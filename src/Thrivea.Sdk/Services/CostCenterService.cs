using Thrivea.Ids;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary><c>/v1/cost_centers</c>.</summary>
public interface ICostCenterService
{
    /// <summary>One page of cost centers, by id; <see cref="CostCenterListOptions.UpdatedAfter" /> narrows it to what changed.</summary>
    Task<ThriveaList<CostCenter>> ListAsync(CostCenterListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Every cost center that matches, page after page.</summary>
    IAsyncEnumerable<CostCenter> ListAutoPagingAsync(CostCenterListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>One cost center by id.</summary>
    Task<CostCenter> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a cost center.</summary>
    Task<CostCenter> CreateAsync(CostCenterCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Changes the fields given.</summary>
    Task<CostCenter> UpdateAsync(string id, CostCenterUpdateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="ICostCenterService" />
public sealed class CostCenterService : ThriveaService, ICostCenterService
{
    private const string BasePath = "/v1/cost_centers";

    internal CostCenterService(ThriveaHttpTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ThriveaList<CostCenter>> ListAsync(CostCenterListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return Transport.GetAsync<ThriveaList<CostCenter>>(BasePath, (options ?? new CostCenterListOptions()).ToQuery(), requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<CostCenter> ListAutoPagingAsync(CostCenterListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return AutoPageAsync<CostCenter, CostCenterListOptions>(options, (page, token) => ListAsync(page, requestOptions, token), cancellationToken);
    }

    /// <inheritdoc />
    public Task<CostCenter> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.CostCenter, nameof(id));
        return Transport.GetAsync<CostCenter>($"{BasePath}/{Uri.EscapeDataString(id)}", null, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<CostCenter> CreateAsync(CostCenterCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Transport.PostJsonAsync<CostCenter>(BasePath, options, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<CostCenter> UpdateAsync(string id, CostCenterUpdateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.CostCenter, nameof(id));
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Transport.PatchJsonAsync<CostCenter>($"{BasePath}/{Uri.EscapeDataString(id)}", options, requestOptions, cancellationToken);
    }
}
