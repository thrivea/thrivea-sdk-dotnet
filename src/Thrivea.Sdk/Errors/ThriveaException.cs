using System.Net;

namespace Thrivea.Errors;

/// <summary>
///     The API answered with an error. Catch a subclass to handle one kind, or this to handle them all; the
///     <see cref="Error" /> carries what the API said and <see cref="StatusCode" /> the HTTP status.
/// </summary>
public class ThriveaException : Exception
{
    /// <summary>A failure with no API answer behind it.</summary>
    public ThriveaException(string message)
        : base(message)
    {
    }

    /// <summary>A failure with no API answer behind it, caused by <paramref name="innerException" />.</summary>
    public ThriveaException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>An error the API returned.</summary>
    public ThriveaException(HttpStatusCode statusCode, ThriveaError error, string? requestId, string? responseBody)
        : base(error?.Message ?? $"The Thrivea API answered {(int)statusCode}.")
    {
        StatusCode = statusCode;
        Error = error;
        RequestId = requestId;
        ResponseBody = responseBody;
    }

    /// <summary>The HTTP status, or <c>null</c> when no answer came back.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>What the API said, or <c>null</c> when no answer came back.</summary>
    public ThriveaError? Error { get; }

    /// <summary>The request id to quote to Thrivea support, when the API sent one.</summary>
    public string? RequestId { get; }

    /// <summary>The raw response body, for logging.</summary>
    public string? ResponseBody { get; }
}

/// <summary>The request never got an answer: a connection failure, a timeout, or a response that could not be read.</summary>
public sealed class ThriveaNetworkException : ThriveaException
{
    /// <summary>Creates the exception.</summary>
    public ThriveaNetworkException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>The API key is missing, wrong, expired or revoked (<c>401</c>).</summary>
public sealed class ThriveaAuthenticationException : ThriveaException
{
    internal ThriveaAuthenticationException(HttpStatusCode statusCode, ThriveaError error, string? requestId, string? responseBody)
        : base(statusCode, error, requestId, responseBody)
    {
    }
}

/// <summary>
///     The key may not do this (<c>403</c>): it lacks the scope, the person it acts for lacks the permission, or the
///     workspace's plan lacks the module. The message says which.
/// </summary>
public sealed class ThriveaPermissionException : ThriveaException
{
    internal ThriveaPermissionException(HttpStatusCode statusCode, ThriveaError error, string? requestId, string? responseBody)
        : base(statusCode, error, requestId, responseBody)
    {
    }
}

/// <summary>A field of the request was invalid (<c>400</c>); <see cref="ThriveaException.Error" />'s <c>Param</c> names it.</summary>
public sealed class ThriveaInvalidRequestException : ThriveaException
{
    internal ThriveaInvalidRequestException(HttpStatusCode statusCode, ThriveaError error, string? requestId, string? responseBody)
        : base(statusCode, error, requestId, responseBody)
    {
    }
}

/// <summary>No such resource in this workspace (<c>404</c>).</summary>
public sealed class ThriveaNotFoundException : ThriveaException
{
    internal ThriveaNotFoundException(HttpStatusCode statusCode, ThriveaError error, string? requestId, string? responseBody)
        : base(statusCode, error, requestId, responseBody)
    {
    }
}

/// <summary>The change conflicts with the current state (<c>409</c>).</summary>
public sealed class ThriveaConflictException : ThriveaException
{
    internal ThriveaConflictException(HttpStatusCode statusCode, ThriveaError error, string? requestId, string? responseBody)
        : base(statusCode, error, requestId, responseBody)
    {
    }
}

/// <summary>Too many requests (<c>429</c>). The SDK already retried; wait for <see cref="RetryAfter" /> before trying again.</summary>
public sealed class ThriveaRateLimitException : ThriveaException
{
    internal ThriveaRateLimitException(HttpStatusCode statusCode, ThriveaError error, string? requestId, string? responseBody, TimeSpan? retryAfter)
        : base(statusCode, error, requestId, responseBody)
    {
        RetryAfter = retryAfter;
    }

    /// <summary>How long the API asked you to wait, when it said.</summary>
    public TimeSpan? RetryAfter { get; }
}

/// <summary>Thrivea failed (<c>5xx</c>). The SDK already retried; try again later and tell support the request id.</summary>
public sealed class ThriveaApiException : ThriveaException
{
    internal ThriveaApiException(HttpStatusCode statusCode, ThriveaError error, string? requestId, string? responseBody)
        : base(statusCode, error, requestId, responseBody)
    {
    }
}
