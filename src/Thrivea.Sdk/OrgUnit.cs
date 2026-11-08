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

    /// <summary>Who manages it, <c>emp_…</c>. Read-only: it comes from each person's work history.</summary>
    [JsonPropertyName("manager_ids")]
    public IReadOnlyList<string> ManagerIds { get; init; } = Array.Empty<string>();

    /// <summary>Who belongs to it, <c>emp_…</c>. Read-only: it comes from each person's work history.</summary>
    [JsonPropertyName("employee_ids")]
    public IReadOnlyList<string> EmployeeIds { get; init; } = Array.Empty<string>();

    /// <summary>The rule behind <see cref="ManagerIds" />, kept for compatibility.</summary>
    [JsonPropertyName("managers_criteria")]
    public PeopleCriteria? ManagersCriteria { get; init; }

    /// <summary>The rule behind <see cref="EmployeeIds" />, kept for compatibility.</summary>
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
///     One rule naming people: <see cref="Type" /> names it (a <see cref="PeopleCriterionTypes" /> value) and the one field
///     that rule takes carries its argument; the other fields stay null and are left out of the JSON.
/// </summary>
public sealed class PeopleCriterion
{
    /// <summary>The rule, such as <c>by_employee_ids</c> or <c>reports_to</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary><c>by_employee_ids</c>: the people named, <c>emp_…</c>.</summary>
    [JsonPropertyName("employee_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? EmployeeIds { get; set; }

    /// <summary><c>is_in_org_unit</c>: the members of that unit, <c>orgu_…</c>.</summary>
    [JsonPropertyName("org_unit_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OrgUnitId { get; set; }

    /// <summary><c>has_job_position</c>: the people in that job position, <c>pos_…</c>.</summary>
    [JsonPropertyName("job_position_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? JobPositionId { get; set; }

    /// <summary><c>works_at_same_site</c>: the people at that site, <c>site_…</c>.</summary>
    [JsonPropertyName("site_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SiteId { get; set; }

    /// <summary><c>is_in_permission_group</c>: the members of that permission group (a GUID; groups have no prefix).</summary>
    [JsonPropertyName("permission_group_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? PermissionGroupId { get; set; }

    /// <summary><c>has_permission</c>: the people who hold that permission, by its stored name.</summary>
    [JsonPropertyName("permission_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PermissionName { get; set; }

    /// <summary><c>has_erb_scalar_field</c>: the people with a value in that employee record field (a GUID; fields have no prefix).</summary>
    [JsonPropertyName("erb_field_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? ErbFieldId { get; set; }

    /// <summary>The fields of a rule this version does not know, as the API sent them.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Properties { get; set; }
}

/// <summary>The rules a <see cref="PeopleCriterion" /> can be. A rule added later arrives as its own snake_case name.</summary>
public static class PeopleCriterionTypes
{
    /// <summary>Everyone in the workspace.</summary>
    public const string Everyone = "everyone";

    /// <summary>Nobody.</summary>
    public const string None = "none";

    /// <summary>The people named in <see cref="PeopleCriterion.EmployeeIds" />.</summary>
    public const string ByEmployeeIds = "by_employee_ids";

    /// <summary>Everyone who reports to the unit's managers.</summary>
    public const string ReportsTo = "reports_to";

    /// <summary>The members of the unit in <see cref="PeopleCriterion.OrgUnitId" />.</summary>
    public const string IsInOrgUnit = "is_in_org_unit";

    /// <summary>The people in the job position in <see cref="PeopleCriterion.JobPositionId" />.</summary>
    public const string HasJobPosition = "has_job_position";

    /// <summary>The people at the site in <see cref="PeopleCriterion.SiteId" />.</summary>
    public const string WorksAtSameSite = "works_at_same_site";

    /// <summary>A person's direct manager.</summary>
    public const string IsDirectManager = "is_direct_manager";

    /// <summary>A person's managers above the direct one.</summary>
    public const string IsIndirectManager = "is_indirect_manager";

    /// <summary>Whoever manages the org unit.</summary>
    public const string IsOrgUnitManager = "is_org_unit_manager";

    /// <summary>The HR business partner.</summary>
    public const string IsHrbp = "is_hrbp";

    /// <summary>The buddy.</summary>
    public const string IsBuddy = "is_buddy";

    /// <summary>The IT administrator.</summary>
    public const string IsItAdmin = "is_it_admin";

    /// <summary>The person themselves.</summary>
    public const string IsSelf = "is_self";

    /// <summary>The members of the permission group in <see cref="PeopleCriterion.PermissionGroupId" />.</summary>
    public const string IsInPermissionGroup = "is_in_permission_group";

    /// <summary>The people who hold the permission in <see cref="PeopleCriterion.PermissionName" />.</summary>
    public const string HasPermission = "has_permission";

    /// <summary>The people with a value in the employee record field in <see cref="PeopleCriterion.ErbFieldId" />.</summary>
    public const string HasErbScalarField = "has_erb_scalar_field";
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

    /// <summary>Refused by the API: managers come from each person's work history.</summary>
    [Obsolete("Who manages and who belongs to an org unit comes from each person's work history; the API refuses this field with a 400.")]
    [JsonPropertyName("manager_ids")]
    public List<string>? ManagerIds { get; set; }

    /// <summary>Refused by the API: members come from each person's work history.</summary>
    [Obsolete("Who manages and who belongs to an org unit comes from each person's work history; the API refuses this field with a 400.")]
    [JsonPropertyName("employee_ids")]
    public List<string>? EmployeeIds { get; set; }

    /// <summary>Who manages it, as rules, instead of <see cref="ManagerIds" />.</summary>
    [Obsolete("Who manages and who belongs to an org unit comes from each person's work history; the API refuses this field with a 400.")]
    [JsonPropertyName("managers_criteria")]
    public PeopleCriteria? ManagersCriteria { get; set; }

    /// <summary>Who is in it, as rules, instead of <see cref="EmployeeIds" />.</summary>
    [Obsolete("Who manages and who belongs to an org unit comes from each person's work history; the API refuses this field with a 400.")]
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
    [Obsolete("Who manages and who belongs to an org unit comes from each person's work history; the API refuses this field with a 400.")]
    [JsonPropertyName("manager_ids")]
    public List<string>? ManagerIds { get; set; }

    /// <summary>Required by the API. Its members, <c>emp_…</c>.</summary>
    [Obsolete("Who manages and who belongs to an org unit comes from each person's work history; the API refuses this field with a 400.")]
    [JsonPropertyName("employee_ids")]
    public List<string>? EmployeeIds { get; set; }

    /// <summary>Who manages it, as rules.</summary>
    [Obsolete("Who manages and who belongs to an org unit comes from each person's work history; the API refuses this field with a 400.")]
    [JsonPropertyName("managers_criteria")]
    public PeopleCriteria? ManagersCriteria { get; set; }

    /// <summary>Who is in it, as rules.</summary>
    [Obsolete("Who manages and who belongs to an org unit comes from each person's work history; the API refuses this field with a 400.")]
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
