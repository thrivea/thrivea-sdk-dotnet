using System.Net;
using Thrivea.Errors;
using Thrivea.Internal;

namespace Thrivea.Sdk.Tests;

public class TransportTests
{
    private const string JobId = "job_01h455vb4pex5vsknk084sn02q";

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, typeof(ThriveaInvalidRequestException), "invalid_request_error")]
    [InlineData(HttpStatusCode.Unauthorized, typeof(ThriveaAuthenticationException), "authentication_error")]
    [InlineData(HttpStatusCode.Forbidden, typeof(ThriveaPermissionException), "permission_error")]
    [InlineData(HttpStatusCode.NotFound, typeof(ThriveaNotFoundException), "resource_missing")]
    [InlineData(HttpStatusCode.Conflict, typeof(ThriveaConflictException), "conflict_error")]
    [InlineData((HttpStatusCode)429, typeof(ThriveaRateLimitException), "rate_limit_error")]
    [InlineData(HttpStatusCode.InternalServerError, typeof(ThriveaApiException), "api_error")]
    [InlineData(HttpStatusCode.BadGateway, typeof(ThriveaApiException), "api_error")]
    public async Task Status_maps_to_the_typed_exception_and_the_error_body_is_kept(HttpStatusCode status, Type exceptionType, string type)
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(status, $$$"""{"error":{"type":"{{{type}}}","message":"Says the API.","param":"id","code":null,"doc_url":null}}""", ("Request-Id", "req_123"));

        var exception = await Assert.ThrowsAsync(exceptionType, () => client.JobOpenings.GetAsync(JobId));

        var thrivea = Assert.IsAssignableFrom<ThriveaException>(exception);
        Assert.Equal(status, thrivea.StatusCode);
        Assert.Equal(type, thrivea.Error!.Type);
        Assert.Equal("Says the API.", thrivea.Message);
        Assert.Equal("id", thrivea.Error.Param);
        Assert.Equal("req_123", thrivea.RequestId);
        Assert.Contains(type, thrivea.ResponseBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_problem_details_body_still_becomes_a_typed_error()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.BadRequest, """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"detail":"The JSON value could not be converted."}""");

        var exception = await Assert.ThrowsAsync<ThriveaInvalidRequestException>(() => client.JobOpenings.CreateAsync(new JobOpeningCreateOptions { Title = "x" }));

        Assert.Equal(ThriveaErrorTypes.InvalidRequest, exception.Error!.Type);
        Assert.Contains("validation errors", exception.Message, StringComparison.Ordinal);
        Assert.Contains("could not be converted", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_non_json_body_still_becomes_a_typed_error()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.BadGateway, "<html>bad gateway</html>");

        var exception = await Assert.ThrowsAsync<ThriveaApiException>(() => client.JobOpenings.GetAsync(JobId));

        Assert.Equal(ThriveaErrorTypes.Api, exception.Error!.Type);
        Assert.Contains("502", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Every_request_carries_the_bearer_key_and_a_user_agent()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"object":"list","url":"/v1/job_openings","data":[],"has_more":false}""");

        await client.JobOpenings.ListAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal("Bearer " + TestClient.ApiKey, request.Headers["Authorization"]);
        Assert.StartsWith("Thrivea.Sdk/", request.Headers["User-Agent"], StringComparison.Ordinal);
        Assert.Equal("application/json", request.Headers["Accept"]);
    }

    [Fact]
    public async Task A_per_request_api_key_replaces_the_clients()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"object":"list","data":[],"has_more":false}""");

        await client.JobOpenings.ListAsync(requestOptions: new RequestOptions { ApiKey = "tk_live_other" });

        Assert.Equal("Bearer tk_live_other", handler.Requests[0].Headers["Authorization"]);
    }

    [Fact]
    public async Task A_get_is_retried_after_a_503_and_the_delay_is_taken()
    {
        var handler = new FakeHttpMessageHandler()
            .Enqueue(HttpStatusCode.ServiceUnavailable, """{"error":{"type":"api_error","message":"down"}}""")
            .Enqueue(HttpStatusCode.OK, """{"object":"job_opening","id":"job_01h455vb4pex5vsknk084sn02q","title":"T","status":"open","workplace_type":"onsite","published":false,"created_at":"2026-11-07T10:00:00Z"}""");
        var delays = new List<TimeSpan>();
        var transport = new ThriveaHttpTransport(
            new HttpClient(handler),
            new ThriveaClientOptions { ApiKey = TestClient.ApiKey, BaseUrl = "https://api.example.test", MaxNetworkRetries = 2 },
            (delay, _) => { delays.Add(delay); return Task.CompletedTask; },
            () => 0.0);

        var job = await transport.GetAsync<JobOpening>("/v1/job_openings/" + JobId, null, null, CancellationToken.None);

        Assert.Equal("T", job.Title);
        Assert.Equal(2, handler.Requests.Count);
        var delay = Assert.Single(delays);
        Assert.Equal(TimeSpan.FromSeconds(0.375), delay);
    }

    [Fact]
    public async Task A_post_without_an_idempotency_key_is_not_retried()
    {
        var (client, handler) = TestClient.Create(maxNetworkRetries: 2);
        handler.Enqueue(HttpStatusCode.ServiceUnavailable, """{"error":{"type":"api_error","message":"down"}}""");

        await Assert.ThrowsAsync<ThriveaApiException>(() => client.JobOpenings.CreateAsync(new JobOpeningCreateOptions { Title = "T", WorkplaceType = WorkplaceType.Onsite }));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task A_post_with_an_idempotency_key_is_retried_and_sends_the_header()
    {
        var handler = new FakeHttpMessageHandler()
            .EnqueueThrow(new HttpRequestException("connection reset"))
            .Enqueue(HttpStatusCode.Created, """{"object":"job_opening","id":"job_01h455vb4pex5vsknk084sn02q","title":"T","status":"open","workplace_type":"onsite","published":false,"created_at":"2026-11-07T10:00:00Z"}""");
        var transport = new ThriveaHttpTransport(
            new HttpClient(handler),
            new ThriveaClientOptions { ApiKey = TestClient.ApiKey, BaseUrl = "https://api.example.test", MaxNetworkRetries = 1 },
            (_, _) => Task.CompletedTask,
            () => 0.0);

        var job = await transport.PostJsonAsync<JobOpening>("/v1/job_openings", new JobOpeningCreateOptions { Title = "T" }, new RequestOptions { IdempotencyKey = "idem-1" }, CancellationToken.None);

        Assert.Equal("T", job.Title);
        Assert.Equal(2, handler.Requests.Count);
        Assert.All(handler.Requests, request => Assert.Equal("idem-1", request.Headers["Idempotency-Key"]));
    }

    [Fact]
    public async Task A_429_waits_at_least_retry_after_then_surfaces_it()
    {
        var handler = new FakeHttpMessageHandler()
            .Enqueue((HttpStatusCode)429, """{"error":{"type":"rate_limit_error","message":"slow down"}}""", ("Retry-After", "3"))
            .Enqueue((HttpStatusCode)429, """{"error":{"type":"rate_limit_error","message":"slow down"}}""", ("Retry-After", "7"));
        var delays = new List<TimeSpan>();
        var transport = new ThriveaHttpTransport(
            new HttpClient(handler),
            new ThriveaClientOptions { ApiKey = TestClient.ApiKey, BaseUrl = "https://api.example.test", MaxNetworkRetries = 1 },
            (delay, _) => { delays.Add(delay); return Task.CompletedTask; },
            () => 0.0);

        var exception = await Assert.ThrowsAsync<ThriveaRateLimitException>(() => transport.GetAsync<JobOpening>("/v1/job_openings/" + JobId, null, null, CancellationToken.None));

        Assert.Equal(TimeSpan.FromSeconds(3), Assert.Single(delays));
        Assert.Equal(TimeSpan.FromSeconds(7), exception.RetryAfter);
    }

    [Fact]
    public async Task A_network_failure_after_the_last_retry_is_a_network_exception()
    {
        var (client, handler) = TestClient.Create();
        handler.EnqueueThrow(new HttpRequestException("no route to host"));

        var exception = await Assert.ThrowsAsync<ThriveaNetworkException>(() => client.JobOpenings.GetAsync(JobId));

        Assert.Contains("no route to host", exception.Message, StringComparison.Ordinal);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task The_callers_cancellation_is_not_swallowed()
    {
        var (client, handler) = TestClient.Create(maxNetworkRetries: 2);
        using var cancellation = new CancellationTokenSource();
        handler.EnqueueThrow(new OperationCanceledException());
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.JobOpenings.GetAsync(JobId, cancellationToken: cancellation.Token));
    }

    [Fact]
    public void Options_are_validated_up_front()
    {
        Assert.Throws<ArgumentException>(() => new ThriveaClient(string.Empty));
        Assert.Throws<ArgumentException>(() => new ThriveaClient(new ThriveaClientOptions { ApiKey = "k", BaseUrl = "not a url" }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ThriveaClient(new ThriveaClientOptions { ApiKey = "k", MaxNetworkRetries = -1 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ThriveaClient(new ThriveaClientOptions { ApiKey = "k", Timeout = TimeSpan.Zero }));
    }
}
