using Thrivea.Ids;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary><c>/v1/job_openings</c>.</summary>
public interface IJobOpeningService
{
    /// <summary>One page of job openings, oldest first; open ones unless <see cref="JobOpeningListOptions.Status" /> says otherwise.</summary>
    Task<ThriveaList<JobOpening>> ListAsync(JobOpeningListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Every job opening that matches, page after page.</summary>
    IAsyncEnumerable<JobOpening> ListAutoPagingAsync(JobOpeningListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>One job opening by id.</summary>
    Task<JobOpening> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a job opening: open, not published, on the workspace's default pipeline, with no hiring team. The same
    ///     <see cref="JobOpeningCreateOptions.ExternalRef" /> again returns the job already created.
    /// </summary>
    Task<JobOpening> CreateAsync(JobOpeningCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IJobOpeningService" />
public sealed class JobOpeningService : ThriveaService, IJobOpeningService
{
    private const string BasePath = "/v1/job_openings";

    internal JobOpeningService(ThriveaHttpTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ThriveaList<JobOpening>> ListAsync(JobOpeningListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return Transport.GetAsync<ThriveaList<JobOpening>>(BasePath, (options ?? new JobOpeningListOptions()).ToQuery(), requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<JobOpening> ListAutoPagingAsync(JobOpeningListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return AutoPageAsync<JobOpening, JobOpeningListOptions>(options, (page, token) => ListAsync(page, requestOptions, token), cancellationToken);
    }

    /// <inheritdoc />
    public Task<JobOpening> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.JobOpening, nameof(id));
        return Transport.GetAsync<JobOpening>($"{BasePath}/{Uri.EscapeDataString(id)}", null, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<JobOpening> CreateAsync(JobOpeningCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Transport.PostJsonAsync<JobOpening>(BasePath, options, requestOptions, cancellationToken);
    }
}
