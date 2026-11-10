using System.Text.Json;
using System.Text.Json.Serialization;
using Thrivea.Internal;

namespace Thrivea.Webhooks;

/// <summary>
///     What Thrivea posts to a webhook endpoint: which event, when, and the event's data. <see cref="Parse" /> reads the
///     body; <see cref="ParseAndVerify" /> checks the signature first.
/// </summary>
public sealed class WebhookEvent
{
    /// <summary>The event's id, the same on every delivery attempt (also the <c>Idempotency-Key</c> header). Ignore an id you have already processed.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>When the event happened.</summary>
    [JsonPropertyName("created")]
    public DateTimeOffset Created { get; init; }

    /// <summary>The event type, such as <c>employee.created</c>; see <see cref="WebhookEventTypes" />.</summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    ///     The event's data. Read it with <see cref="GetData{T}" />. A <c>job_opening.*</c> event carries the
    ///     <see cref="JobOpening" /> as the API returns it; the older events carry the property names Thrivea records (PascalCase).
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement Data { get; init; }

    /// <summary>Reads the body of a webhook request.</summary>
    public static WebhookEvent Parse(string json)
    {
        if (json is null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        return ThriveaJson.Deserialize<WebhookEvent>(json) ?? throw new JsonException("The webhook body is empty.");
    }

    /// <summary>Verifies the signature (see <see cref="WebhookSignature.Verify" />), then reads the body.</summary>
    public static WebhookEvent ParseAndVerify(string json, string? signature, string? timestamp, string secret, TimeSpan? tolerance = null)
    {
        WebhookSignature.Verify(json, signature, timestamp, secret, tolerance);
        return Parse(json);
    }

    /// <summary>Deserializes <see cref="Data" /> into your own type; property names are matched case-insensitively.</summary>
    public T? GetData<T>()
    {
        return Data.ValueKind == JsonValueKind.Undefined ? default : Data.Deserialize<T>(ThriveaJson.Options);
    }
}
