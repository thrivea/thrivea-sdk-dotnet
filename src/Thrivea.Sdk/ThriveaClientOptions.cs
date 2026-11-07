namespace Thrivea;

/// <summary>
///     How a <see cref="ThriveaClient" /> reaches the Thrivea API: the key, the host, the HTTP client and the retry policy.
/// </summary>
public sealed class ThriveaClientOptions
{
    /// <summary>The production API host.</summary>
    public const string ProductionBaseUrl = "https://api.thrivea.com";

    /// <summary>
    ///     Your API key, <c>tk_live_…</c>. Minted in Thrivea with the scopes the integration needs (for a careers website:
    ///     <c>read:job_openings</c>, <c>write:job_openings</c>, <c>write:candidate_applications</c>).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    ///     The API host, without a trailing slash. Defaults to <see cref="ProductionBaseUrl" />; a Thrivea engineer can
    ///     point you at a development host for testing.
    /// </summary>
    public string BaseUrl { get; set; } = ProductionBaseUrl;

    /// <summary>
    ///     An <see cref="System.Net.Http.HttpClient" /> to send with, for example one from <c>IHttpClientFactory</c>. When
    ///     left <c>null</c> the client creates and owns its own.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    ///     How many times a request is retried after a network failure, a <c>409</c>, a <c>429</c> or a <c>5xx</c> answer.
    ///     Reads and deletes are always retried; a <c>POST</c> or <c>PATCH</c> only when the request carries an
    ///     <see cref="RequestOptions.IdempotencyKey" />. Defaults to 2; 0 switches retries off.
    /// </summary>
    public int MaxNetworkRetries { get; set; } = 2;

    /// <summary>How long one attempt may take before it counts as a network failure. Defaults to 30 seconds.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new ArgumentException("ApiKey is required: a Thrivea API key such as tk_live_….", nameof(ApiKey));
        }

        if (string.IsNullOrWhiteSpace(BaseUrl) || !Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http"))
        {
            throw new ArgumentException("BaseUrl must be an absolute http(s) URL such as https://api.thrivea.com.", nameof(BaseUrl));
        }

        if (MaxNetworkRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxNetworkRetries), "MaxNetworkRetries cannot be negative.");
        }

        if (Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(Timeout), "Timeout must be positive.");
        }
    }
}
