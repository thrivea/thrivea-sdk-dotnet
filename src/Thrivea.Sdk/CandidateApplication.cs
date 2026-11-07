using System.Text.Json.Serialization;

namespace Thrivea;

/// <summary>An application a candidate made to one job opening.</summary>
public sealed class CandidateApplication : IThriveaResource
{
    /// <summary>Always <c>candidate_application</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "candidate_application";

    /// <summary>The id, <c>capp_…</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The job opening applied to, <c>job_…</c>.</summary>
    [JsonPropertyName("job_opening_id")]
    public string JobOpeningId { get; init; } = string.Empty;

    /// <summary>The candidate, <c>cand_…</c>. The same person applying again, by e-mail, is the same candidate.</summary>
    [JsonPropertyName("candidate_id")]
    public string CandidateId { get; init; } = string.Empty;

    /// <summary>Your id for the submission, as you sent it.</summary>
    [JsonPropertyName("external_ref")]
    public string? ExternalRef { get; init; }

    /// <summary>Whether a CV was stored with the application.</summary>
    [JsonPropertyName("has_cv")]
    public bool HasCv { get; init; }

    /// <summary>When the application was made.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
///     What an application needs. The candidate is found by e-mail or created; the tags are found by name or created and
///     added to the candidate's own; the application lands in the job's first pipeline stage and the hiring team is told.
/// </summary>
public sealed class CandidateApplicationCreateOptions
{
    /// <summary>Required. An open job opening, <c>job_…</c>.</summary>
    [JsonPropertyName("job_opening_id")]
    public string? JobOpeningId { get; set; }

    /// <summary>Required.</summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>Required.</summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    /// <summary>Required. The candidate with this e-mail is reused; a second application to the same job returns the first.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>The candidate's phone number.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>What the candidate wrote, and any form answers that have no field of their own.</summary>
    [JsonPropertyName("cover_note")]
    public string? CoverNote { get; set; }

    /// <summary>Tag names such as <c>#frontend-engineer</c>: found by name (the <c>#</c> is optional), created when missing.</summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>Your id for the submission, stored on the application.</summary>
    [JsonPropertyName("external_ref")]
    public string? ExternalRef { get; set; }

    /// <summary>The CV, optional: a PDF, a Word document or plain text. Sent as <c>multipart/form-data</c> when present.</summary>
    [JsonIgnore]
    public FileUpload? Cv { get; set; }
}
