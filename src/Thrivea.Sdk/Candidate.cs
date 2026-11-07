using System.Text.Json.Serialization;
using Thrivea.Internal;

namespace Thrivea;

/// <summary>A person who applied or was sourced: one record per person in the workspace, matched by e-mail.</summary>
public sealed class Candidate : IThriveaResource
{
    /// <summary>Always <c>candidate</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "candidate";

    /// <summary>The id, <c>cand_…</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>First name.</summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Last name.</summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>The e-mail the candidate is known by.</summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>Phone number, when given.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; init; }

    /// <summary>A one-line headline, when the recruiter wrote one.</summary>
    [JsonPropertyName("headline")]
    public string? Headline { get; init; }

    /// <summary>The candidate's tags, by name, such as <c>#frontend-engineer</c>.</summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>When the record was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the record or its tags last changed.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Filters for listing candidates.</summary>
public sealed class CandidateListOptions : ListOptions
{
    /// <summary>Only the candidate with this e-mail, exactly as stored.</summary>
    public string? Email { get; set; }

    /// <summary>Only those created or changed after this moment, for an incremental sync.</summary>
    public DateTimeOffset? UpdatedAfter { get; set; }

    internal override void WriteTo(QueryStringBuilder query)
    {
        base.WriteTo(query);
        query.Add("email", Email).Add("updated_after", UpdatedAfter);
    }
}
