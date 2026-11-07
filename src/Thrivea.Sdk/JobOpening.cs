using System.Text.Json.Serialization;
using Thrivea.Internal;

namespace Thrivea;

/// <summary>A role a company is hiring for, as another system needs it: to list, to pick, and to attach applications to.</summary>
public sealed class JobOpening : IThriveaResource
{
    /// <summary>Always <c>job_opening</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "job_opening";

    /// <summary>The id, <c>job_…</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The title, as a candidate reads it.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>One of <see cref="JobOpeningStatus" />. Applications are accepted only while <c>open</c>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>One of <see cref="WorkplaceType" />.</summary>
    [JsonPropertyName("workplace_type")]
    public string WorkplaceType { get; init; } = string.Empty;

    /// <summary>Whether the job is on the Thrivea careers page. A job created through the API is not, until a recruiter publishes it.</summary>
    [JsonPropertyName("published")]
    public bool Published { get; init; }

    /// <summary>The id your system gave the job when it created it, if it did.</summary>
    [JsonPropertyName("external_ref")]
    public string? ExternalRef { get; init; }

    /// <summary>When the job was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>The values of <see cref="JobOpening.Status" />. Treat a value you do not know as one that was added later.</summary>
public static class JobOpeningStatus
{
    /// <summary>Hiring; accepts applications.</summary>
    public const string Open = "open";

    /// <summary>Hiring is over.</summary>
    public const string Closed = "closed";

    /// <summary>Put away.</summary>
    public const string Archived = "archived";
}

/// <summary>The values of <see cref="JobOpening.WorkplaceType" />.</summary>
public static class WorkplaceType
{
    /// <summary>At the company's site.</summary>
    public const string Onsite = "onsite";

    /// <summary>From anywhere.</summary>
    public const string Remote = "remote";

    /// <summary>Part on site, part remote.</summary>
    public const string Hybrid = "hybrid";
}

/// <summary>Filters for listing job openings.</summary>
public sealed class JobOpeningListOptions : ListOptions
{
    /// <summary>One of <see cref="JobOpeningStatus" />. The API lists <c>open</c> ones when this is left out.</summary>
    public string? Status { get; set; }

    internal override void WriteTo(QueryStringBuilder query)
    {
        query.Add("status", Status);
        base.WriteTo(query);
    }
}

/// <summary>What a new job opening needs. It is created open, unpublished, on the workspace's default pipeline, with no hiring team.</summary>
public sealed class JobOpeningCreateOptions
{
    /// <summary>Required. The title, as a candidate reads it.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>Plain text or basic HTML. May be the ad's summary and a link to its page on your site.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Required. One of <see cref="Thrivea.WorkplaceType" />.</summary>
    [JsonPropertyName("workplace_type")]
    public string? WorkplaceType { get; set; }

    /// <summary>Your id for the job (the ad's id in your CMS). Sending the same one again returns the job already created, never a second one.</summary>
    [JsonPropertyName("external_ref")]
    public string? ExternalRef { get; set; }
}
