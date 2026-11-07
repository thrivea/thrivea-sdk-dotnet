using Thrivea.Services;

namespace Thrivea;

/// <summary>
///     The Thrivea API, one service per resource. Register <see cref="ThriveaClient" /> behind this interface and your own
///     code stays testable without the network.
/// </summary>
public interface IThriveaClient
{
    /// <summary><c>/v1/job_openings</c>: the roles a company is hiring for.</summary>
    IJobOpeningService JobOpenings { get; }

    /// <summary><c>/v1/candidate_applications</c>: an application a candidate makes to a job opening.</summary>
    ICandidateApplicationService CandidateApplications { get; }

    /// <summary><c>/v1/candidates</c>: the people who applied or were sourced.</summary>
    ICandidateService Candidates { get; }

    /// <summary><c>/v1/employees</c>: the people records.</summary>
    IEmployeeService Employees { get; }

    /// <summary><c>/v1/org_units</c>: departments, teams and other organizational units.</summary>
    IOrgUnitService OrgUnits { get; }

    /// <summary><c>/v1/org_unit_types</c>: the kinds of organizational unit a company uses.</summary>
    IOrgUnitTypeService OrgUnitTypes { get; }

    /// <summary><c>/v1/cost_centers</c>: cost centers and the job positions charged to them.</summary>
    ICostCenterService CostCenters { get; }

    /// <summary><c>/v1/webhook_endpoints</c>: where Thrivea sends events.</summary>
    IWebhookEndpointService WebhookEndpoints { get; }
}
