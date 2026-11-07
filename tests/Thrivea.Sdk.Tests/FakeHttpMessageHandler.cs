using System.Net;
using System.Text;

namespace Thrivea.Sdk.Tests;

/// <summary>Answers requests from a queue and keeps what was sent, so a test reads the wire without a server.</summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<RecordedRequest> Requests { get; } = new();

    public FakeHttpMessageHandler Enqueue(HttpStatusCode status, string body, params (string Name, string Value)[] headers)
    {
        _responses.Enqueue(_ =>
        {
            var response = new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
            foreach (var (name, value) in headers)
            {
                response.Headers.TryAddWithoutValidation(name, value);
            }

            return response;
        });
        return this;
    }

    public FakeHttpMessageHandler EnqueueThrow(Exception exception)
    {
        _responses.Enqueue(_ => throw exception);
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var recorded = new RecordedRequest
        {
            Method = request.Method.Method,
            Url = request.RequestUri!.ToString(),
            Headers = request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase),
            ContentType = request.Content?.Headers.ContentType?.ToString(),
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken)
        };
        Requests.Add(recorded);

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException($"No response queued for {recorded.Method} {recorded.Url}.");
        }

        return _responses.Dequeue()(request);
    }

    public sealed class RecordedRequest
    {
        public string Method { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public Dictionary<string, string> Headers { get; init; } = new();
        public string? ContentType { get; init; }
        public string? Body { get; init; }
    }
}

public static class TestClient
{
    public const string ApiKey = "tk_live_00000000-0000-0000-0000-000000000000_testkey";

    public static (ThriveaClient Client, FakeHttpMessageHandler Handler) Create(int maxNetworkRetries = 0)
    {
        var handler = new FakeHttpMessageHandler();
        var client = new ThriveaClient(new ThriveaClientOptions
        {
            ApiKey = ApiKey,
            BaseUrl = "https://api.example.test",
            HttpClient = new HttpClient(handler),
            MaxNetworkRetries = maxNetworkRetries
        });
        return (client, handler);
    }
}
