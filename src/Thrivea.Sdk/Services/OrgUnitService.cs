using Thrivea.Ids;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary><c>/v1/org_units</c>.</summary>
public interface IOrgUnitService
{
    /// <summary>One page of org units.</summary>
    Task<ThriveaList<OrgUnit>> ListAsync(OrgUnitListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Every org unit, page after page.</summary>
    IAsyncEnumerable<OrgUnit> ListAutoPagingAsync(OrgUnitListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>One org unit by id.</summary>
    Task<OrgUnit> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Creates an org unit.</summary>
    Task<OrgUnit> CreateAsync(OrgUnitCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Changes an org unit; see <see cref="OrgUnitUpdateOptions" /> for what the API requires.</summary>
    Task<OrgUnit> UpdateAsync(string id, OrgUnitUpdateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes an org unit.</summary>
    Task<DeletedResource> DeleteAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IOrgUnitService" />
public sealed class OrgUnitService : ThriveaService, IOrgUnitService
{
    private const string BasePath = "/v1/org_units";

    internal OrgUnitService(ThriveaHttpTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ThriveaList<OrgUnit>> ListAsync(OrgUnitListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return Transport.GetAsync<ThriveaList<OrgUnit>>(BasePath, (options ?? new OrgUnitListOptions()).ToQuery(), requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<OrgUnit> ListAutoPagingAsync(OrgUnitListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return AutoPageAsync<OrgUnit, OrgUnitListOptions>(options, (page, token) => ListAsync(page, requestOptions, token), cancellationToken);
    }

    /// <inheritdoc />
    public Task<OrgUnit> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.OrgUnit, nameof(id));
        return Transport.GetAsync<OrgUnit>($"{BasePath}/{Uri.EscapeDataString(id)}", null, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OrgUnit> CreateAsync(OrgUnitCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Transport.PostJsonAsync<OrgUnit>(BasePath, options, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OrgUnit> UpdateAsync(string id, OrgUnitUpdateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.OrgUnit, nameof(id));
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Transport.PatchJsonAsync<OrgUnit>($"{BasePath}/{Uri.EscapeDataString(id)}", options, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DeletedResource> DeleteAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.OrgUnit, nameof(id));
        return Transport.DeleteAsync<DeletedResource>($"{BasePath}/{Uri.EscapeDataString(id)}", requestOptions, cancellationToken);
    }
}
