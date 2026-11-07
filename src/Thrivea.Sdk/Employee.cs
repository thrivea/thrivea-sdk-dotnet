using System.Text.Json.Serialization;
using Thrivea.Internal;

namespace Thrivea;

/// <summary>A person record.</summary>
public sealed class Employee : IThriveaResource
{
    /// <summary>Always <c>employee</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "employee";

    /// <summary>The id, <c>emp_…</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>First name.</summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Middle name, when recorded and when the key may read it.</summary>
    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; init; }

    /// <summary>Last name.</summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>The name shown in Thrivea, when it differs from first and last.</summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; init; }

    /// <summary>Work e-mail.</summary>
    [JsonPropertyName("email_address")]
    public string? EmailAddress { get; init; }

    /// <summary>Profile photo URL.</summary>
    [JsonPropertyName("profile_picture_url")]
    public string? ProfilePictureUrl { get; init; }

    /// <summary>Date of birth, only when the key may read it.</summary>
    [JsonPropertyName("birth_date")]
    public DateTimeOffset? BirthDate { get; init; }

    /// <summary>Cover photo URL.</summary>
    [JsonPropertyName("profile_cover_picture_url")]
    public string? ProfileCoverPictureUrl { get; init; }

    /// <summary>Where the person is in their lifecycle.</summary>
    [JsonPropertyName("status")]
    public EmployeeStatus Status { get; init; }
}

/// <summary>Where an employee is in their lifecycle.</summary>
[JsonConverter(typeof(TolerantEnumConverter<EmployeeStatus>))]
public enum EmployeeStatus
{
    /// <summary>A value this version of the SDK does not know.</summary>
    Unknown = 0,

    /// <summary>Invited, has not accepted yet.</summary>
    Invited = 1,

    /// <summary>Working.</summary>
    Active = 2,

    /// <summary>Deactivated.</summary>
    Deactivated = 3,

    /// <summary>Scheduled to be deactivated.</summary>
    PendingDeactivation = 4
}

/// <summary>Paging for employees.</summary>
public sealed class EmployeeListOptions : ListOptions
{
}
