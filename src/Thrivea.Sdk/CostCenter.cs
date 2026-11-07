using System.Text.Json.Serialization;
using Thrivea.Internal;

namespace Thrivea;

/// <summary>A cost center and the job positions charged to it.</summary>
public sealed class CostCenter : IThriveaResource
{
    /// <summary>Always <c>cost_center</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "cost_center";

    /// <summary>The id, <c>cc_…</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The code, unique in the workspace.</summary>
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    /// <summary>The job positions charged to it, <c>pos_…</c>.</summary>
    [JsonPropertyName("job_position_ids")]
    public IReadOnlyList<string> JobPositionIds { get; init; } = Array.Empty<string>();

    /// <summary>Whether it is in use.</summary>
    [JsonPropertyName("active")]
    public bool Active { get; init; }

    /// <summary>When it was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When it last changed.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Filters for listing cost centers.</summary>
public sealed class CostCenterListOptions : ListOptions
{
    /// <summary>Only those changed after this moment, for an incremental sync.</summary>
    public DateTimeOffset? UpdatedAfter { get; set; }

    internal override void WriteTo(QueryStringBuilder query)
    {
        base.WriteTo(query);
        query.Add("updated_after", UpdatedAfter);
    }
}

/// <summary>What a new cost center needs.</summary>
public sealed class CostCenterCreateOptions
{
    /// <summary>Required. Unique in the workspace.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>The job positions charged to it, <c>pos_…</c>.</summary>
    [JsonPropertyName("job_position_ids")]
    public List<string>? JobPositionIds { get; set; }
}

/// <summary>The fields of a cost center to change; a field left <c>null</c> is not sent.</summary>
public sealed class CostCenterUpdateOptions
{
    /// <summary>The code.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>The whole list of job positions charged to it, <c>pos_…</c>.</summary>
    [JsonPropertyName("job_position_ids")]
    public List<string>? JobPositionIds { get; set; }
}
