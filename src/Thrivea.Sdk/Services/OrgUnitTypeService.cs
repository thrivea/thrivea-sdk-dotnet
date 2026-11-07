using Thrivea.Ids;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary><c>/v1/org_unit_types</c>.</summary>
public interface IOrgUnitTypeService
{
    /// <summary>One page of org-unit types.</summary>
    Task<ThriveaList<OrgUnitType>> ListAsync(OrgUnitTypeListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Every org-unit type, page after page.</summary>
    IAsyncEnumerable<OrgUnitType> ListAutoPagingAsync(OrgUnitTypeListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>One org-unit type by id.</summary>
    Task<OrgUnitType> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Creates an org-unit type.</summary>
    Task<OrgUnitType> CreateAsync(OrgUnitTypeCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Changes the fields given.</summary>
    Task<OrgUnitType> UpdateAsync(string id, OrgUnitTypeUpdateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes an org-unit type.</summary>
    Task<DeletedResource> DeleteAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IOrgUnitTypeService" />
public sealed class OrgUnitTypeService : ThriveaService, IOrgUnitTypeService
{
    private const string BasePath = "/v1/org_unit_types";

    internal OrgUnitTypeService(ThriveaHttpTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ThriveaList<OrgUnitType>> ListAsync(OrgUnitTypeListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return Transport.GetAsync<ThriveaList<OrgUnitType>>(BasePath, (options ?? new OrgUnitTypeListOptions()).ToQuery(), requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<OrgUnitType> ListAutoPagingAsync(OrgUnitTypeListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return AutoPageAsync<OrgUnitType, OrgUnitTypeListOptions>(options, (page, token) => ListAsync(page, requestOptions, token), cancellationToken);
    }

    /// <inheritdoc />
    public Task<OrgUnitType> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.OrgUnitType, nameof(id));
        return Transport.GetAsync<OrgUnitType>($"{BasePath}/{Uri.EscapeDataString(id)}", null, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OrgUnitType> CreateAsync(OrgUnitTypeCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Transport.PostJsonAsync<OrgUnitType>(BasePath, options, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OrgUnitType> UpdateAsync(string id, OrgUnitTypeUpdateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.OrgUnitType, nameof(id));
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Transport.PatchJsonAsync<OrgUnitType>($"{BasePath}/{Uri.EscapeDataString(id)}", options, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DeletedResource> DeleteAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.OrgUnitType, nameof(id));
        return Transport.DeleteAsync<DeletedResource>($"{BasePath}/{Uri.EscapeDataString(id)}", requestOptions, cancellationToken);
    }
}
