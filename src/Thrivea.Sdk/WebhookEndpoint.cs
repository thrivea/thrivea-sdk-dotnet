using System.Text.Json.Serialization;

namespace Thrivea;

/// <summary>A URL Thrivea sends events to.</summary>
public sealed class WebhookEndpoint : IThriveaResource
{
    /// <summary>Always <c>webhook_endpoint</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "webhook_endpoint";

    /// <summary>The id, <c>we_…</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>Where events are posted.</summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    /// <summary>What it is for.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Whether events are sent. Answering <c>410 Gone</c> switches an endpoint off.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; }

    /// <summary>The events it receives; see <see cref="WebhookEventTypes" />.</summary>
    [JsonPropertyName("event_types")]
    public IReadOnlyList<string> EventTypes { get; init; } = Array.Empty<string>();

    /// <summary>When it was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When it last changed.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    ///     The signing secret, set only on the endpoint <c>CreateAsync</c> returns: Thrivea shows it once and cannot show it
    ///     again. Store it; <see cref="Webhooks.WebhookSignature" /> needs it to verify deliveries.
    /// </summary>
    [JsonIgnore]
    public string? Secret { get; internal set; }
}

/// <summary>The events a webhook endpoint can subscribe to. <c>ListEventTypesAsync</c> gives the live list.</summary>
public static class WebhookEventTypes
{
    /// <summary>An employee was created.</summary>
    public const string EmployeeCreated = "employee.created";

    /// <summary>An employee changed.</summary>
    public const string EmployeeUpdated = "employee.updated";

    /// <summary>An employee was deleted.</summary>
    public const string EmployeeDeleted = "employee.deleted";

    /// <summary>An org unit was created.</summary>
    public const string OrgUnitCreated = "org_unit.created";

    /// <summary>An org unit changed.</summary>
    public const string OrgUnitUpdated = "org_unit.updated";

    /// <summary>An org unit was deleted.</summary>
    public const string OrgUnitDeleted = "org_unit.deleted";

    /// <summary>An org-unit type was created.</summary>
    public const string OrgUnitTypeCreated = "org_unit_type.created";

    /// <summary>An org-unit type changed.</summary>
    public const string OrgUnitTypeUpdated = "org_unit_type.updated";

    /// <summary>An org-unit type was deleted.</summary>
    public const string OrgUnitTypeDeleted = "org_unit_type.deleted";

    /// <summary>A job opening was created. The data is the <see cref="JobOpening" />; read it with <c>GetData&lt;JobOpening&gt;()</c>.</summary>
    public const string JobOpeningCreated = "job_opening.created";

    /// <summary>A job opening's title, description, workplace type, sites or positions changed. The data is the <see cref="JobOpening" />.</summary>
    public const string JobOpeningUpdated = "job_opening.updated";

    /// <summary>A job opening went on the careers page. The data is the <see cref="JobOpening" />.</summary>
    public const string JobOpeningPublished = "job_opening.published";

    /// <summary>A job opening came off the careers page. The data is the <see cref="JobOpening" />.</summary>
    public const string JobOpeningUnpublished = "job_opening.unpublished";

    /// <summary>A job opening was closed and takes no more applications. The data is the <see cref="JobOpening" />.</summary>
    public const string JobOpeningClosed = "job_opening.closed";

    /// <summary>A closed job opening was opened again. The data is the <see cref="JobOpening" />.</summary>
    public const string JobOpeningReopened = "job_opening.reopened";

    /// <summary>A job opening was archived. The data is the <see cref="JobOpening" />.</summary>
    public const string JobOpeningArchived = "job_opening.archived";

    /// <summary>A person joined (identity provisioning).</summary>
    public const string UserJoiner = "user.joiner";

    /// <summary>A person moved (identity provisioning).</summary>
    public const string UserMover = "user.mover";

    /// <summary>A person left (identity provisioning).</summary>
    public const string UserLeaver = "user.leaver";

    /// <summary>A person was rehired (identity provisioning).</summary>
    public const string UserRehire = "user.rehire";
}

/// <summary>Paging for webhook endpoints.</summary>
public sealed class WebhookEndpointListOptions : ListOptions
{
}

/// <summary>What a new webhook endpoint needs.</summary>
public sealed class WebhookEndpointCreateOptions
{
    /// <summary>Required. An https URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>What it is for.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>The events to receive; see <see cref="WebhookEventTypes" />.</summary>
    [JsonPropertyName("event_types")]
    public List<string> EventTypes { get; set; } = new();

    /// <summary>Whether to start sending at once.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

/// <summary>
///     The fields of a webhook endpoint to change. The API reads a missing <see cref="Enabled" /> as <c>true</c>, so send
///     the current value when you mean to keep it.
/// </summary>
public sealed class WebhookEndpointUpdateOptions
{
    /// <summary>An https URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>What it is for.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>The whole list of events to receive.</summary>
    [JsonPropertyName("event_types")]
    public List<string>? EventTypes { get; set; }

    /// <summary>Whether events are sent.</summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}

internal sealed class WebhookEventTypesEnvelope
{
    [JsonPropertyName("object")]
    public string Object { get; init; } = "webhook_event_types";

    [JsonPropertyName("data")]
    public List<string> Data { get; init; } = new();
}
