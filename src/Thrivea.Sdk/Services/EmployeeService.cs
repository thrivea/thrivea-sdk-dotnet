using Thrivea.Ids;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary><c>/v1/employees</c>, read-only.</summary>
public interface IEmployeeService
{
    /// <summary>One page of employees.</summary>
    Task<ThriveaList<Employee>> ListAsync(EmployeeListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Every employee, page after page.</summary>
    IAsyncEnumerable<Employee> ListAutoPagingAsync(EmployeeListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>One employee by id.</summary>
    Task<Employee> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IEmployeeService" />
public sealed class EmployeeService : ThriveaService, IEmployeeService
{
    private const string BasePath = "/v1/employees";

    internal EmployeeService(ThriveaHttpTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ThriveaList<Employee>> ListAsync(EmployeeListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return Transport.GetAsync<ThriveaList<Employee>>(BasePath, (options ?? new EmployeeListOptions()).ToQuery(), requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<Employee> ListAutoPagingAsync(EmployeeListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return AutoPageAsync<Employee, EmployeeListOptions>(options, (page, token) => ListAsync(page, requestOptions, token), cancellationToken);
    }

    /// <inheritdoc />
    public Task<Employee> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.Employee, nameof(id));
        return Transport.GetAsync<Employee>($"{BasePath}/{Uri.EscapeDataString(id)}", null, requestOptions, cancellationToken);
    }
}
