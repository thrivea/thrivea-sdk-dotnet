using System.Net;
using System.Text.Json;

namespace Thrivea.Sdk.Tests;

public class JobOpeningServiceTests
{
    private const string JobJson = """{"object":"job_opening","id":"job_01h455vb4pex5vsknk084sn02q","title":"Frontend Engineer","status":"open","workplace_type":"onsite","published":false,"external_ref":"cms-42","created_at":"2026-11-07T10:00:00+00:00"}""";

    [Fact]
    public async Task List_sends_the_filters_as_snake_case_query_parameters()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, $$"""{"object":"list","url":"/v1/job_openings","data":[{{JobJson}}],"has_more":true}""");

        var page = await client.JobOpenings.ListAsync(new JobOpeningListOptions { Status = JobOpeningStatus.Closed, Limit = 50, StartingAfter = "job_01h455vb4pex5vsknk084sn02q" });

        Assert.Equal("https://api.example.test/v1/job_openings?status=closed&limit=50&starting_after=job_01h455vb4pex5vsknk084sn02q", handler.Requests[0].Url);
        Assert.True(page.HasMore);
        var job = Assert.Single(page.Data);
        Assert.Equal("Frontend Engineer", job.Title);
        Assert.Equal(JobOpeningStatus.Open, job.Status);
        Assert.Equal(WorkplaceType.Onsite, job.WorkplaceType);
        Assert.Equal("cms-42", job.ExternalRef);
        Assert.Equal(new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero), job.CreatedAt);
    }

    [Fact]
    public async Task List_without_options_sends_no_query()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"object":"list","data":[],"has_more":false}""");

        await client.JobOpenings.ListAsync();

        Assert.Equal("https://api.example.test/v1/job_openings", handler.Requests[0].Url);
    }

    [Fact]
    public async Task Get_checks_the_prefix_before_sending()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, JobJson);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => client.JobOpenings.GetAsync("cand_01h455vb4pex5vsknk084sn02q"));
        Assert.Empty(handler.Requests);
        Assert.Equal("id", error.ParamName);

        var job = await client.JobOpenings.GetAsync("job_01h455vb4pex5vsknk084sn02q");
        Assert.Equal("https://api.example.test/v1/job_openings/job_01h455vb4pex5vsknk084sn02q", handler.Requests[0].Url);
        Assert.Equal("job_01h455vb4pex5vsknk084sn02q", job.Id);
    }

    [Fact]
    public async Task Create_posts_snake_case_json_and_leaves_null_fields_out()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.Created, JobJson);

        var job = await client.JobOpenings.CreateAsync(new JobOpeningCreateOptions
        {
            Title = "Frontend Engineer",
            WorkplaceType = WorkplaceType.Onsite,
            ExternalRef = "cms-42"
        });

        var request = Assert.Single(handler.Requests);
        Assert.Equal("POST", request.Method);
        Assert.StartsWith("application/json", request.ContentType, StringComparison.Ordinal);
        using var body = JsonDocument.Parse(request.Body!);
        Assert.Equal("Frontend Engineer", body.RootElement.GetProperty("title").GetString());
        Assert.Equal("onsite", body.RootElement.GetProperty("workplace_type").GetString());
        Assert.Equal("cms-42", body.RootElement.GetProperty("external_ref").GetString());
        Assert.False(body.RootElement.TryGetProperty("description", out _));
        Assert.Equal("job_01h455vb4pex5vsknk084sn02q", job.Id);
        Assert.False(job.Published);
    }

    [Fact]
    public async Task Auto_paging_walks_every_page_from_the_last_id()
    {
        var (client, handler) = TestClient.Create();
        handler
            .Enqueue(HttpStatusCode.OK, """{"object":"list","data":[{"object":"job_opening","id":"job_00000000000000000000000001","title":"A","status":"open","workplace_type":"remote","published":false,"created_at":"2026-01-01T00:00:00Z"},{"object":"job_opening","id":"job_00000000000000000000000002","title":"B","status":"open","workplace_type":"remote","published":false,"created_at":"2026-01-01T00:00:00Z"}],"has_more":true}""")
            .Enqueue(HttpStatusCode.OK, """{"object":"list","data":[{"object":"job_opening","id":"job_00000000000000000000000003","title":"C","status":"open","workplace_type":"remote","published":false,"created_at":"2026-01-01T00:00:00Z"}],"has_more":false}""");

        var titles = new List<string>();
        await foreach (var job in client.JobOpenings.ListAutoPagingAsync(new JobOpeningListOptions { Status = JobOpeningStatus.Open }))
        {
            titles.Add(job.Title);
        }

        Assert.Equal(["A", "B", "C"], titles);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("https://api.example.test/v1/job_openings?status=open&limit=100", handler.Requests[0].Url);
        Assert.Equal("https://api.example.test/v1/job_openings?status=open&limit=100&starting_after=job_00000000000000000000000002", handler.Requests[1].Url);
    }

    [Fact]
    public async Task An_unknown_status_value_is_kept_as_the_string_the_api_sent()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, JobJson.Replace("\"open\"", "\"paused\"", StringComparison.Ordinal));

        var job = await client.JobOpenings.GetAsync("job_01h455vb4pex5vsknk084sn02q");

        Assert.Equal("paused", job.Status);
    }
}
