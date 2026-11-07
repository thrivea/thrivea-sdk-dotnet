using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Thrivea.Errors;

namespace Thrivea.Internal;

/// <summary>
///     Sends one API request: builds the URL and headers, retries what is safe to retry, and turns a non-2xx answer
///     into the matching <see cref="ThriveaException" />.
/// </summary>
internal sealed class ThriveaHttpTransport
{
    private static readonly string UserAgent = BuildUserAgent();

    private readonly HttpClient _httpClient;
    private readonly ThriveaClientOptions _options;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly Func<double> _jitter;

    public ThriveaHttpTransport(HttpClient httpClient, ThriveaClientOptions options)
        : this(httpClient, options, Task.Delay, NextJitter)
    {
    }

    /// <summary>For tests: the delay and the jitter are injected so retries are observed without sleeping.</summary>
    internal ThriveaHttpTransport(HttpClient httpClient, ThriveaClientOptions options, Func<TimeSpan, CancellationToken, Task> delay, Func<double> jitter)
    {
        _httpClient = httpClient;
        _options = options;
        _delay = delay;
        _jitter = jitter;
    }

    public Task<TResponse> GetAsync<TResponse>(string path, QueryStringBuilder? query, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return SendAsync<TResponse>(HttpMethod.Get, path + (query?.ToString() ?? string.Empty), null, requestOptions, cancellationToken);
    }

    public Task<TResponse> PostJsonAsync<TResponse>(string path, object body, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        var json = ThriveaJson.Serialize(body);
        return SendAsync<TResponse>(HttpMethod.Post, path, () => new StringContent(json, Encoding.UTF8, "application/json"), requestOptions, cancellationToken);
    }

    /// <summary>A POST whose answer carries something in a header too (a webhook endpoint's one-time secret).</summary>
    public Task<ApiResponse<TResponse>> PostJsonWithResponseAsync<TResponse>(string path, object body, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        var json = ThriveaJson.Serialize(body);
        return SendCoreAsync<TResponse>(HttpMethod.Post, path, () => new StringContent(json, Encoding.UTF8, "application/json"), requestOptions, cancellationToken);
    }

    public Task<TResponse> PatchJsonAsync<TResponse>(string path, object body, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        var json = ThriveaJson.Serialize(body);
        return SendAsync<TResponse>(new HttpMethod("PATCH"), path, () => new StringContent(json, Encoding.UTF8, "application/json"), requestOptions, cancellationToken);
    }

    public Task<TResponse> PostMultipartAsync<TResponse>(string path, Func<MultipartFormDataContent> contentFactory, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return SendAsync<TResponse>(HttpMethod.Post, path, contentFactory, requestOptions, cancellationToken);
    }

    public Task<TResponse> DeleteAsync<TResponse>(string path, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return SendAsync<TResponse>(HttpMethod.Delete, path, null, requestOptions, cancellationToken);
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string pathAndQuery,
        Func<HttpContent>? contentFactory,
        RequestOptions? requestOptions,
        CancellationToken cancellationToken)
    {
        var response = await SendCoreAsync<TResponse>(method, pathAndQuery, contentFactory, requestOptions, cancellationToken).ConfigureAwait(false);
        return response.Value;
    }

    private async Task<ApiResponse<TResponse>> SendCoreAsync<TResponse>(
        HttpMethod method,
        string pathAndQuery,
        Func<HttpContent>? contentFactory,
        RequestOptions? requestOptions,
        CancellationToken cancellationToken)
    {
        var url = _options.BaseUrl.TrimEnd('/') + pathAndQuery;
        var apiKey = requestOptions?.ApiKey ?? _options.ApiKey;
        var idempotencyKey = requestOptions?.IdempotencyKey;
        var retryable = IsRetryableMethod(method) || !string.IsNullOrEmpty(idempotencyKey);
        var maxAttempts = 1 + (retryable ? _options.MaxNetworkRetries : 0);

        for (var attempt = 1; ; attempt++)
        {
            using var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                request.Headers.TryAddWithoutValidation("Idempotency-Key", idempotencyKey);
            }

            if (contentFactory is not null)
            {
                request.Content = contentFactory();
            }

            HttpResponseMessage? response = null;
            string body;
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(_options.Timeout);
                try
                {
                    response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
                    body = await ReadBodyAsync(response, timeout.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException exception)
                {
                    response?.Dispose();
                    if (attempt < maxAttempts)
                    {
                        await BackOffAsync(attempt, null, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    throw new ThriveaNetworkException($"The request to {method} {pathAndQuery} did not complete within {_options.Timeout.TotalSeconds:0} seconds.", exception);
                }
                catch (HttpRequestException exception)
                {
                    response?.Dispose();
                    if (attempt < maxAttempts)
                    {
                        await BackOffAsync(attempt, null, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    throw new ThriveaNetworkException($"The request to {method} {pathAndQuery} failed: {exception.Message}", exception);
                }

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<TResponse>(Deserialize<TResponse>(body, response), CollectHeaders(response));
                }

                var retryAfter = ReadRetryAfter(response);
                if (attempt < maxAttempts && IsRetryableStatus(response.StatusCode))
                {
                    await BackOffAsync(attempt, retryAfter, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                throw CreateException(response, body, retryAfter);
            }
            finally
            {
                response?.Dispose();
            }
        }
    }

    private static TResponse Deserialize<TResponse>(string body, HttpResponseMessage response)
    {
        try
        {
            var value = ThriveaJson.Deserialize<TResponse>(body);
            if (value is null)
            {
                throw new ThriveaNetworkException($"The API answered {(int)response.StatusCode} with an empty body.", null);
            }

            return value;
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new ThriveaNetworkException($"The API answered {(int)response.StatusCode} with a body that is not the expected JSON.", exception);
        }
    }

    private static ThriveaException CreateException(HttpResponseMessage response, string body, TimeSpan? retryAfter)
    {
        var requestId = ReadHeader(response, "Request-Id");
        var error = ParseError(body, response.StatusCode);
        requestId ??= error.RequestId;

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ThriveaInvalidRequestException(response.StatusCode, error, requestId, body),
            HttpStatusCode.Unauthorized => new ThriveaAuthenticationException(response.StatusCode, error, requestId, body),
            HttpStatusCode.Forbidden => new ThriveaPermissionException(response.StatusCode, error, requestId, body),
            HttpStatusCode.NotFound => new ThriveaNotFoundException(response.StatusCode, error, requestId, body),
            HttpStatusCode.Conflict => new ThriveaConflictException(response.StatusCode, error, requestId, body),
            (HttpStatusCode)429 => new ThriveaRateLimitException(response.StatusCode, error, requestId, body, retryAfter),
            _ when (int)response.StatusCode >= 500 => new ThriveaApiException(response.StatusCode, error, requestId, body),
            _ => new ThriveaException(response.StatusCode, error, requestId, body)
        };
    }

    /// <summary>
    ///     Reads the API's error envelope. A body that is not one (a proxy's HTML, ASP.NET's problem details for a
    ///     malformed JSON body) still becomes a <see cref="ThriveaError" />, typed by the status.
    /// </summary>
    internal static ThriveaError ParseError(string body, HttpStatusCode statusCode)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var envelope = ThriveaJson.Deserialize<ThriveaErrorEnvelope>(body);
                if (envelope?.Error is not null && !string.IsNullOrEmpty(envelope.Error.Type))
                {
                    return envelope.Error;
                }

                var problem = ThriveaJson.Deserialize<ProblemDetailsShape>(body);
                if (problem is not null && (!string.IsNullOrEmpty(problem.Title) || !string.IsNullOrEmpty(problem.Detail)))
                {
                    return new ThriveaError
                    {
                        Type = DefaultType(statusCode),
                        Message = string.IsNullOrEmpty(problem.Detail) ? problem.Title! : $"{problem.Title}: {problem.Detail}".TrimStart(':', ' ')
                    };
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // Not JSON: fall through to the status-only error.
            }
        }

        return new ThriveaError { Type = DefaultType(statusCode), Message = $"The Thrivea API answered {(int)statusCode} {statusCode}." };
    }

    private static string DefaultType(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => ThriveaErrorTypes.InvalidRequest,
            HttpStatusCode.Unauthorized => ThriveaErrorTypes.Authentication,
            HttpStatusCode.Forbidden => ThriveaErrorTypes.Permission,
            HttpStatusCode.NotFound => ThriveaErrorTypes.ResourceMissing,
            HttpStatusCode.Conflict => ThriveaErrorTypes.Conflict,
            (HttpStatusCode)429 => ThriveaErrorTypes.RateLimit,
            _ => ThriveaErrorTypes.Api
        };
    }

    private static bool IsRetryableMethod(HttpMethod method)
    {
        return method == HttpMethod.Get || method == HttpMethod.Delete || method == HttpMethod.Head;
    }

    private static bool IsRetryableStatus(HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return code == 409 || code == 429 || code >= 500;
    }

    private async Task BackOffAsync(int attempt, TimeSpan? retryAfter, CancellationToken cancellationToken)
    {
        // Exponential: 0.5 s, 1 s, 2 s, … capped at 8 s, with up to 25 % jitter so retries from many servers spread out;
        // never shorter than what the API asked for in Retry-After.
        var seconds = Math.Min(8, 0.5 * Math.Pow(2, attempt - 1));
        var delay = TimeSpan.FromSeconds(seconds * (0.75 + _jitter() * 0.25));
        if (retryAfter.HasValue && retryAfter.Value > delay)
        {
            delay = retryAfter.Value;
        }

        await _delay(delay, cancellationToken).ConfigureAwait(false);
    }

    private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null)
        {
            return null;
        }

        if (retryAfter.Delta.HasValue)
        {
            return retryAfter.Delta.Value;
        }

        if (retryAfter.Date.HasValue)
        {
            var wait = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
        }

        return null;
    }

    private static Dictionary<string, string> CollectHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        return headers;
    }

    private static string? ReadHeader(HttpResponseMessage response, string name)
    {
        return response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
    }

    private static async Task<string> ReadBodyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content is null)
        {
            return string.Empty;
        }

#if NETSTANDARD2_0
        cancellationToken.ThrowIfCancellationRequested();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
    }

    private static readonly Random Random = new();

    private static double NextJitter()
    {
        lock (Random)
        {
            return Random.NextDouble();
        }
    }

    private static string BuildUserAgent()
    {
        var version = typeof(ThriveaHttpTransport).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                      ?? typeof(ThriveaHttpTransport).Assembly.GetName().Version?.ToString()
                      ?? "0";
        var plus = version.IndexOf('+');
        if (plus > 0)
        {
            version = version.Substring(0, plus);
        }

        return string.Format(CultureInfo.InvariantCulture, "Thrivea.Sdk/{0} ({1}; {2})", version, RuntimeInformation.FrameworkDescription, RuntimeInformation.OSDescription);
    }
}

/// <summary>A successful answer: the body, and the headers for the rare call that needs one.</summary>
internal sealed class ApiResponse<T>
{
    public ApiResponse(T value, IReadOnlyDictionary<string, string> headers)
    {
        Value = value;
        Headers = headers;
    }

    public T Value { get; }

    public IReadOnlyDictionary<string, string> Headers { get; }

    public string? Header(string name)
    {
        return Headers.TryGetValue(name, out var value) ? value : null;
    }
}

/// <summary>ASP.NET's problem-details body, which a malformed JSON request gets instead of the API's own error envelope.</summary>
internal sealed class ProblemDetailsShape
{
    public string? Title { get; init; }

    public string? Detail { get; init; }
}
