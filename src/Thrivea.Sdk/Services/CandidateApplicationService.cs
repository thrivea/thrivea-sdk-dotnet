using System.Net.Http.Headers;
using System.Text;
using Thrivea.Ids;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary><c>/v1/candidate_applications</c>.</summary>
public interface ICandidateApplicationService
{
    /// <summary>
    ///     Submits an application. The candidate is found by e-mail or created, the tags are added, the CV is stored when
    ///     given, and the hiring team is told. The same candidate and job again returns the application already made, so a
    ///     website that retries makes one.
    /// </summary>
    Task<CandidateApplication> CreateAsync(CandidateApplicationCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="ICandidateApplicationService" />
public sealed class CandidateApplicationService : ThriveaService, ICandidateApplicationService
{
    private const string BasePath = "/v1/candidate_applications";

    internal CandidateApplicationService(ThriveaHttpTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public async Task<CandidateApplication> CreateAsync(CandidateApplicationCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        TypeId.EnsurePrefix(options.JobOpeningId, ResourcePrefixes.JobOpening, nameof(options.JobOpeningId));

        if (options.Cv is null)
        {
            return await Transport.PostJsonAsync<CandidateApplication>(BasePath, options, requestOptions, cancellationToken).ConfigureAwait(false);
        }

        // The form is built per attempt (an HttpContent cannot be sent twice), from bytes read once.
        var cv = await options.Cv.ReadAsync(cancellationToken).ConfigureAwait(false);
        return await Transport.PostMultipartAsync<CandidateApplication>(BasePath, () => BuildForm(options, cv), requestOptions, cancellationToken).ConfigureAwait(false);
    }

    internal static MultipartFormDataContent BuildForm(CandidateApplicationCreateOptions options, byte[] cv)
    {
        var form = new MultipartFormDataContent();
        AddField(form, "job_opening_id", options.JobOpeningId);
        AddField(form, "first_name", options.FirstName);
        AddField(form, "last_name", options.LastName);
        AddField(form, "email", options.Email);
        AddField(form, "phone", options.Phone);
        AddField(form, "cover_note", options.CoverNote);
        AddField(form, "external_ref", options.ExternalRef);
        foreach (var tag in options.Tags)
        {
            AddField(form, "tags", tag);
        }

        var file = new ByteArrayContent(cv);
        file.Headers.ContentType = new MediaTypeHeaderValue(options.Cv!.ContentType);
        form.Add(file, "cv", options.Cv.FileName);
        return form;
    }

    private static void AddField(MultipartFormDataContent form, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            form.Add(new StringContent(value!, Encoding.UTF8), name);
        }
    }
}
