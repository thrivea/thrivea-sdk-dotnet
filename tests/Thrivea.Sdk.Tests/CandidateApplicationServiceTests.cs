using System.Net;
using System.Text;
using System.Text.Json;

namespace Thrivea.Sdk.Tests;

public class CandidateApplicationServiceTests
{
    private const string ApplicationJson = """{"object":"candidate_application","id":"capp_01h455vb4pex5vsknk084sn02q","job_opening_id":"job_01h455vb4pex5vsknk084sn02q","candidate_id":"cand_01h455vb4pex5vsknk084sn02q","external_ref":"form-7","has_cv":true,"created_at":"2026-11-07T10:00:00Z"}""";

    [Fact]
    public async Task Without_a_cv_the_application_is_posted_as_json()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.Created, ApplicationJson);

        var application = await client.CandidateApplications.CreateAsync(new CandidateApplicationCreateOptions
        {
            JobOpeningId = "job_01h455vb4pex5vsknk084sn02q",
            FirstName = "Ana",
            LastName = "Anić",
            Email = "ana@example.com",
            Tags = { "#frontend-engineer", "#novi-sad" },
            ExternalRef = "form-7"
        });

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.example.test/v1/candidate_applications", request.Url);
        Assert.StartsWith("application/json", request.ContentType, StringComparison.Ordinal);
        using var body = JsonDocument.Parse(request.Body!);
        Assert.Equal("Anić", body.RootElement.GetProperty("last_name").GetString());
        Assert.Equal(2, body.RootElement.GetProperty("tags").GetArrayLength());
        Assert.False(body.RootElement.TryGetProperty("phone", out _));
        Assert.False(body.RootElement.TryGetProperty("cv", out _));
        Assert.Equal("capp_01h455vb4pex5vsknk084sn02q", application.Id);
        Assert.Equal("cand_01h455vb4pex5vsknk084sn02q", application.CandidateId);
        Assert.True(application.HasCv);
    }

    [Fact]
    public async Task With_a_cv_the_application_is_posted_as_multipart_with_the_file_and_repeated_tags()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.Created, ApplicationJson);
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.4 fake");

        await client.CandidateApplications.CreateAsync(new CandidateApplicationCreateOptions
        {
            JobOpeningId = "job_01h455vb4pex5vsknk084sn02q",
            FirstName = "Ana",
            LastName = "Anić",
            Email = "ana@example.com",
            CoverNote = "Hello",
            Tags = { "#frontend-engineer", "#novi-sad" },
            Cv = FileUpload.FromBytes(pdf, "ana-cv.pdf", "application/pdf")
        });

        var request = Assert.Single(handler.Requests);
        Assert.StartsWith("multipart/form-data", request.ContentType, StringComparison.Ordinal);
        Assert.Contains("name=job_opening_id", request.Body, StringComparison.Ordinal);
        Assert.Contains("name=cover_note", request.Body, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(request.Body!, "name=tags"));
        Assert.Contains("name=cv; filename=ana-cv.pdf", request.Body, StringComparison.Ordinal);
        Assert.Contains("Content-Type: application/pdf", request.Body, StringComparison.Ordinal);
        Assert.Contains("%PDF-1.4 fake", request.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_cv_from_a_stream_is_read_once_and_can_be_resent()
    {
        var handler = new FakeHttpMessageHandler()
            .EnqueueThrow(new HttpRequestException("reset"))
            .Enqueue(HttpStatusCode.OK, ApplicationJson);
        var client = new ThriveaClient(new ThriveaClientOptions { ApiKey = TestClient.ApiKey, BaseUrl = "https://api.example.test", HttpClient = new HttpClient(handler), MaxNetworkRetries = 1 });
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes("plain text cv"));

        var application = await client.CandidateApplications.CreateAsync(
            new CandidateApplicationCreateOptions { JobOpeningId = "job_01h455vb4pex5vsknk084sn02q", FirstName = "A", LastName = "B", Email = "a@b.c", Cv = new FileUpload(stream, "cv.txt", "text/plain") },
            new RequestOptions { IdempotencyKey = "form-7" });

        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request => Assert.Contains("plain text cv", request.Body, StringComparison.Ordinal));
        Assert.Equal("capp_01h455vb4pex5vsknk084sn02q", application.Id);
    }

    [Fact]
    public async Task A_200_for_the_application_already_made_is_a_success()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, ApplicationJson);

        var application = await client.CandidateApplications.CreateAsync(new CandidateApplicationCreateOptions { JobOpeningId = "job_01h455vb4pex5vsknk084sn02q", FirstName = "A", LastName = "B", Email = "a@b.c" });

        Assert.Equal("form-7", application.ExternalRef);
    }

    [Fact]
    public async Task An_application_updated_with_a_new_cv_says_so_and_names_the_version_now_current()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"object":"candidate_application","id":"capp_01h455vb4pex5vsknk084sn02q","job_opening_id":"job_01h455vb4pex5vsknk084sn02q","candidate_id":"cand_01h455vb4pex5vsknk084sn02q","has_cv":true,"cv_version":2,"updated":true,"created_at":"2024-03-01T10:00:00Z"}""");

        var application = await client.CandidateApplications.CreateAsync(new CandidateApplicationCreateOptions { JobOpeningId = "job_01h455vb4pex5vsknk084sn02q", FirstName = "A", LastName = "B", Email = "a@b.c" });

        Assert.True(application.Updated);
        Assert.Equal(2, application.CvVersion);
    }

    [Fact]
    public async Task The_job_opening_id_prefix_is_checked_before_sending()
    {
        var (client, handler) = TestClient.Create();

        var error = await Assert.ThrowsAsync<ArgumentException>(() => client.CandidateApplications.CreateAsync(new CandidateApplicationCreateOptions { JobOpeningId = "42", FirstName = "A", LastName = "B", Email = "a@b.c" }));

        Assert.Equal("JobOpeningId", error.ParamName);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public void Content_type_follows_the_extension()
    {
        Assert.Equal("application/pdf", FileUpload.ContentTypeFor("cv.PDF"));
        Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document", FileUpload.ContentTypeFor("cv.docx"));
        Assert.Equal("text/plain", FileUpload.ContentTypeFor("cv.txt"));
        Assert.Equal("application/octet-stream", FileUpload.ContentTypeFor("cv.rtf"));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
