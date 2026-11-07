using Thrivea.Ids;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary><c>/v1/candidates</c>, read-only. A candidate is created through <see cref="ICandidateApplicationService" />.</summary>
public interface ICandidateService
{
    /// <summary>One page of candidates, oldest first; <see cref="CandidateListOptions.Email" /> finds one person.</summary>
    Task<ThriveaList<Candidate>> ListAsync(CandidateListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Every candidate that matches, page after page.</summary>
    IAsyncEnumerable<Candidate> ListAutoPagingAsync(CandidateListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>One candidate by id, such as the <c>candidate_id</c> an application returned.</summary>
    Task<Candidate> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="ICandidateService" />
public sealed class CandidateService : ThriveaService, ICandidateService
{
    private const string BasePath = "/v1/candidates";

    internal CandidateService(ThriveaHttpTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ThriveaList<Candidate>> ListAsync(CandidateListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return Transport.GetAsync<ThriveaList<Candidate>>(BasePath, (options ?? new CandidateListOptions()).ToQuery(), requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<Candidate> ListAutoPagingAsync(CandidateListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return AutoPageAsync<Candidate, CandidateListOptions>(options, (page, token) => ListAsync(page, requestOptions, token), cancellationToken);
    }

    /// <inheritdoc />
    public Task<Candidate> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.Candidate, nameof(id));
        return Transport.GetAsync<Candidate>($"{BasePath}/{Uri.EscapeDataString(id)}", null, requestOptions, cancellationToken);
    }
}
