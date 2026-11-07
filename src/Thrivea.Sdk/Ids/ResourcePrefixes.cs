namespace Thrivea.Ids;

/// <summary>
///     The prefix of every Thrivea identifier, as in <c>job_01h2xcejqtf2nbrexx3vqjhp41</c>. A prefix is permanent: once
///     shipped it never changes, so these are safe to store and to branch on.
/// </summary>
public static class ResourcePrefixes
{
    /// <summary>An employee, <c>emp_…</c>.</summary>
    public const string Employee = "emp";

    /// <summary>An employment (one work-history entry), <c>empl_…</c>.</summary>
    public const string Employment = "empl";

    /// <summary>A job position (a title in the catalogue), <c>pos_…</c>.</summary>
    public const string JobPosition = "pos";

    /// <summary>An organizational unit, <c>orgu_…</c>.</summary>
    public const string OrgUnit = "orgu";

    /// <summary>An org-unit type, <c>orgut_…</c>.</summary>
    public const string OrgUnitType = "orgut";

    /// <summary>A site (office, location), <c>site_…</c>.</summary>
    public const string Site = "site";

    /// <summary>A time-off request, <c>tor_…</c>.</summary>
    public const string TimeOffRequest = "tor";

    /// <summary>A time-off policy, <c>top_…</c>.</summary>
    public const string TimeOffPolicy = "top";

    /// <summary>A time-off balance, <c>tob_…</c>.</summary>
    public const string TimeOffBalance = "tob";

    /// <summary>A company (the authenticated workspace), <c>co_…</c>.</summary>
    public const string Company = "co";

    /// <summary>An API key record (not the secret itself), <c>key_…</c>.</summary>
    public const string ApiKey = "key";

    /// <summary>A webhook endpoint, <c>we_…</c>.</summary>
    public const string WebhookEndpoint = "we";

    /// <summary>A webhook event, <c>evt_…</c>.</summary>
    public const string Event = "evt";

    /// <summary>A job opening (a role being hired for), <c>job_…</c>.</summary>
    public const string JobOpening = "job";

    /// <summary>A saved report view, <c>rep_…</c>.</summary>
    public const string ReportView = "rep";

    /// <summary>A cost center, <c>cc_…</c>.</summary>
    public const string CostCenter = "cc";

    /// <summary>A candidate (a person who applied or was sourced), <c>cand_…</c>.</summary>
    public const string Candidate = "cand";

    /// <summary>A candidate's application to one job opening, <c>capp_…</c>.</summary>
    public const string CandidateApplication = "capp";
}
