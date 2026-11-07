using System.Text.Json.Serialization;

namespace Thrivea.Errors;

/// <summary>The error the API returned, as in <c>{ "error": { "type": …, "message": …, "param": … } }</c>.</summary>
public sealed class ThriveaError
{
    /// <summary>The stable category to branch on; one of <see cref="ThriveaErrorTypes" />.</summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>A finer, stable code when the API gives one.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>A sentence for a person or a log. Never parse it.</summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    /// <summary>The request field at fault, when the error is about one.</summary>
    [JsonPropertyName("param")]
    public string? Param { get; init; }

    /// <summary>Where to read more, when the API gives a link.</summary>
    [JsonPropertyName("doc_url")]
    public string? DocUrl { get; init; }

    /// <summary>The request's id for Thrivea support, when the API gives one.</summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; init; }
}

/// <summary>The error types the API uses. Branch on these, never on the message.</summary>
public static class ThriveaErrorTypes
{
    /// <summary>The request was malformed or a field was invalid (<c>400</c>). <see cref="ThriveaError.Param" /> names it.</summary>
    public const string InvalidRequest = "invalid_request_error";

    /// <summary>The API key is missing, wrong, expired or revoked (<c>401</c>).</summary>
    public const string Authentication = "authentication_error";

    /// <summary>The key lacks the scope, the person it acts for lacks the permission, or the workspace lacks the module (<c>403</c>).</summary>
    public const string Permission = "permission_error";

    /// <summary>No such resource for this workspace (<c>404</c>).</summary>
    public const string ResourceMissing = "resource_missing";

    /// <summary>The change conflicts with the current state (<c>409</c>).</summary>
    public const string Conflict = "conflict_error";

    /// <summary>Too many requests (<c>429</c>); wait for <see cref="ThriveaRateLimitException.RetryAfter" />.</summary>
    public const string RateLimit = "rate_limit_error";

    /// <summary>Thrivea failed (<c>5xx</c>). Retrying later usually works; tell support with the request id.</summary>
    public const string Api = "api_error";
}

internal sealed class ThriveaErrorEnvelope
{
    [JsonPropertyName("error")]
    public ThriveaError? Error { get; init; }
}
