namespace Thrivea.Webhooks;

/// <summary>
///     The <c>data</c> of an <c>employee.created</c> event: who was added. Thrivea sends the property names in PascalCase
///     and <see cref="WebhookEvent.GetData{T}" /> matches them case-insensitively. Fetch the employee for the rest.
/// </summary>
/// <param name="EmployeeId">The employee's id as a GUID; <c>TypeId.FromUuid(ResourcePrefixes.Employee, EmployeeId)</c> gives the API's form.</param>
/// <param name="EmailAddress">The work e-mail the person was added with.</param>
public sealed record EmployeeCreatedData(Guid EmployeeId, string EmailAddress);
