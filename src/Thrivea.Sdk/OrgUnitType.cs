using System.Text.Json.Serialization;
using Thrivea.Internal;

namespace Thrivea;

/// <summary>A kind of organizational unit: department, team, division…</summary>
public sealed class OrgUnitType : IThriveaResource
{
    /// <summary>Always <c>org_unit_type</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "org_unit_type";

    /// <summary>The id, <c>orgut_…</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>What it is for.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>The colour it is drawn in, as <c>#RRGGBB</c>.</summary>
    [JsonPropertyName("hex_color")]
    public string HexColor { get; init; } = string.Empty;

    /// <summary>Whether it is in use.</summary>
    [JsonPropertyName("status")]
    public OrgUnitTypeStatus Status { get; init; }

    /// <summary>When it was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When it last changed.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>Whether an org-unit type is in use.</summary>
[JsonConverter(typeof(TolerantEnumConverter<OrgUnitTypeStatus>))]
public enum OrgUnitTypeStatus
{
    /// <summary>A value this version of the SDK does not know.</summary>
    Unknown = 0,

    /// <summary>In use.</summary>
    Active = 1,

    /// <summary>Put away.</summary>
    Archived = 2
}

/// <summary>Paging for org-unit types.</summary>
public sealed class OrgUnitTypeListOptions : ListOptions
{
}

/// <summary>What a new org-unit type needs.</summary>
public sealed class OrgUnitTypeCreateOptions
{
    /// <summary>Required.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>What it is for.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Required. <c>#RRGGBB</c>.</summary>
    [JsonPropertyName("hex_color")]
    public string? HexColor { get; set; }
}

/// <summary>The fields of an org-unit type to change; a field left <c>null</c> is not sent.</summary>
public sealed class OrgUnitTypeUpdateOptions
{
    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>What it is for.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary><c>#RRGGBB</c>.</summary>
    [JsonPropertyName("hex_color")]
    public string? HexColor { get; set; }

    /// <summary>Archive or reactivate.</summary>
    [JsonPropertyName("status")]
    public OrgUnitTypeStatus? Status { get; set; }
}
