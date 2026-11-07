using System.Text.Json;
using System.Text.Json.Serialization;
using Thrivea.Internal;

namespace Thrivea;

/// <summary>A department, team, division or other unit of the organization.</summary>
public sealed class OrgUnit : IThriveaResource
{
    /// <summary>Always <c>org_unit</c>.</summary>
    [JsonPropertyName("object")]
    public string Object { get; init; } = "org_unit";

    /// <summary>The id, <c>orgu_…</c>.</summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>The name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>What it is.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Its types, <c>orgut_…</c>.</summary>
    [JsonPropertyName("org_unit_type_ids")]
    public IReadOnlyList<string> OrgUnitTypeIds { get; init; } = Array.Empty<string>();

    /// <summary>The units above it, <c>orgu_…</c>.</summary>
    [JsonPropertyName("parent_org_unit_ids")]
    public IReadOnlyList<string> ParentOrgUnitIds { get; init; } = Array.Empty<string>();

    /// <summary>Units beside it, <c>orgu_…</c>.</summary>
    [JsonPropertyName("sibling_org_unit_ids")]
    public IReadOnlyList<string> SiblingOrgUnitIds { get; init; } = Array.Empty<string>();

    /// <summary>Who manages it.</summary>
    [JsonPropertyName("managers_criteria")]
    public PeopleCriteria? ManagersCriteria { get; init; }

    /// <summary>Who is in it.</summary>
    [JsonPropertyName("employees_criteria")]
    public PeopleCriteria? EmployeesCriteria { get; init; }

    /// <summary>Whether it is in use.</summary>
    [JsonPropertyName("status")]
    public OrgUnitStatus Status { get; init; }

    /// <summary>From when it exists.</summary>
    [JsonPropertyName("active_from")]
    public DateTimeOffset? ActiveFrom { get; init; }

    /// <summary>Until when it exists.</summary>
    [JsonPropertyName("active_to")]
    public DateTimeOffset? ActiveTo { get; init; }

    /// <summary>When it was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Who created it, <c>emp_…</c>.</summary>
    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; init; }

    /// <summary>When it last changed.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>Who last changed it, <c>emp_…</c>.</summary>
    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; init; }

    /// <summary>Your own code for it, when you gave one.</summary>
    [JsonPropertyName("external_code")]
    public string? ExternalCode { get; init; }
}

/// <summary>Whether an org unit is in use.</summary>
[JsonConverter(typeof(TolerantEnumConverter<OrgUnitStatus>))]
public enum OrgUnitStatus
{
    /// <summary>A value this version of the SDK does not know.</summary>
    Unknown = 0,

    /// <summary>In use.</summary>
    Active = 1,

    /// <summary>Put away.</summary>
    Archived = 2,

    /// <summary>Not yet active.</summary>
    Pending = 3,

    /// <summary>Deleted.</summary>
    Deleted = 4
}

/// <summary>
///     Who belongs to a group: the people matched by any of <see cref="Includes" /> and by none of <see cref="Excludes" />.
/// </summary>
public sealed class PeopleCriteria
{
    /// <summary>Criteria that bring people in.</summary>
    [JsonPropertyName("includes")]
    public List<PeopleCriterion> Includes { get; set; } = new();

    /// <summary>Criteria that take people out.</summary>
    [JsonPropertyName("excludes")]
    public List<PeopleCriterion> Excludes { get; set; } = new();
}

/// <summary>
///     One rule naming people, by its <see cref="Type" /> (see <see cref="PeopleCriterionTypes" />) and the type's own
///     fields, kept in <see cref="Properties" /> as the API sends them.
/// </summary>
public sealed class PeopleCriterion
{
    /// <summary>The rule, such as <c>Everyone</c> or <c>ByEmployeeIds</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>The rule's own fields, by name.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Properties { get; set; }
}

/// <summary>The rules a <see cref="PeopleCriterion" /> can be.</summary>
public static class PeopleCriterionTypes
{
    /// <summary>Everyone in the workspace.</summary>
    public const string Everyone = "Everyone";

    /// <summary>Nobody.</summary>
    public const string None = "None";

    /// <summary>Named employees.</summary>
    public const string ByEmployeeIds = "ByEmployeeIds";

    /// <summary>Whoever reports to a given person.</summary>
    public const string ReportsTo = "ReportsTo";

    /// <summary>Whoever is in a given org unit.</summary>
    public const string IsInOrgUnit = "IsInOrgUnit";

    /// <summary>Whoever holds a given job position.</summary>
    public const string HasJobPosition = "HasJobPosition";

    /// <summary>Whoever works at the same site.</summary>
    public const string WorksAtSameSite = "WorksAtSameSite";

    /// <summary>Whoever manages a given org unit.</summary>
    public const string IsOrgUnitManager = "IsOrgUnitManager";
}

/// <summary>Paging for org units.</summary>
public sealed class OrgUnitListOptions : ListOptions
{
}

/// <summary>What a new org unit needs.</summary>
public sealed class OrgUnitCreateOptions
{
    /// <summary>Required.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>What it is.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Its types, <c>orgut_…</c>.</summary>
    [JsonPropertyName("org_unit_type_ids")]
    public List<string>? OrgUnitTypeIds { get; set; }

    /// <summary>The units above it, <c>orgu_…</c>.</summary>
    [JsonPropertyName("parent_org_unit_ids")]
    public List<string>? ParentOrgUnitIds { get; set; }

    /// <summary>Units beside it, <c>orgu_…</c>.</summary>
    [JsonPropertyName("sibling_org_unit_ids")]
    public List<string>? SiblingOrgUnitIds { get; set; }

    /// <summary>Its managers, <c>emp_…</c>.</summary>
    [JsonPropertyName("manager_ids")]
    public List<string>? ManagerIds { get; set; }

    /// <summary>Its members, <c>emp_…</c>.</summary>
    [JsonPropertyName("employee_ids")]
    public List<string>? EmployeeIds { get; set; }

    /// <summary>Who manages it, as rules, instead of <see cref="ManagerIds" />.</summary>
    [JsonPropertyName("managers_criteria")]
    public PeopleCriteria? ManagersCriteria { get; set; }

    /// <summary>Who is in it, as rules, instead of <see cref="EmployeeIds" />.</summary>
    [JsonPropertyName("employees_criteria")]
    public PeopleCriteria? EmployeesCriteria { get; set; }

    /// <summary>A public id of your choosing.</summary>
    [JsonPropertyName("public_id")]
    public string? PublicId { get; set; }

    /// <summary>From when it exists.</summary>
    [JsonPropertyName("active_from")]
    public DateTimeOffset? ActiveFrom { get; set; }

    /// <summary>Until when it exists.</summary>
    [JsonPropertyName("active_to")]
    public DateTimeOffset? ActiveTo { get; set; }

    /// <summary>Your own code for it.</summary>
    [JsonPropertyName("external_code")]
    public string? ExternalCode { get; set; }
}

/// <summary>
///     The fields of an org unit to change. The API requires <see cref="Name" />, <see cref="OrgUnitTypeIds" /> and
///     <see cref="EmployeeIds" /> on every update; read the unit first and send them back.
/// </summary>
public sealed class OrgUnitUpdateOptions
{
    /// <summary>Required by the API.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>What it is.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Required by the API. Its types, <c>orgut_…</c>.</summary>
    [JsonPropertyName("org_unit_type_ids")]
    public List<string>? OrgUnitTypeIds { get; set; }

    /// <summary>The units above it, <c>orgu_…</c>.</summary>
    [JsonPropertyName("parent_org_unit_ids")]
    public List<string>? ParentOrgUnitIds { get; set; }

    /// <summary>Units beside it, <c>orgu_…</c>.</summary>
    [JsonPropertyName("sibling_org_unit_ids")]
    public List<string>? SiblingOrgUnitIds { get; set; }

    /// <summary>Its managers, <c>emp_…</c>.</summary>
    [JsonPropertyName("manager_ids")]
    public List<string>? ManagerIds { get; set; }

    /// <summary>Required by the API. Its members, <c>emp_…</c>.</summary>
    [JsonPropertyName("employee_ids")]
    public List<string>? EmployeeIds { get; set; }

    /// <summary>Who manages it, as rules.</summary>
    [JsonPropertyName("managers_criteria")]
    public PeopleCriteria? ManagersCriteria { get; set; }

    /// <summary>Who is in it, as rules.</summary>
    [JsonPropertyName("employees_criteria")]
    public PeopleCriteria? EmployeesCriteria { get; set; }

    /// <summary>Archive, reactivate…</summary>
    [JsonPropertyName("status")]
    public OrgUnitStatus? Status { get; set; }

    /// <summary>A public id of your choosing.</summary>
    [JsonPropertyName("public_id")]
    public string? PublicId { get; set; }

    /// <summary>From when it exists.</summary>
    [JsonPropertyName("active_from")]
    public DateTimeOffset? ActiveFrom { get; set; }

    /// <summary>Until when it exists.</summary>
    [JsonPropertyName("active_to")]
    public DateTimeOffset? ActiveTo { get; set; }

    /// <summary>Your own code for it.</summary>
    [JsonPropertyName("external_code")]
    public string? ExternalCode { get; set; }
}
