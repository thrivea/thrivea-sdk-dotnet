using Thrivea.Internal;
using Thrivea.Services;

namespace Thrivea;

/// <summary>
///     The entry point: <c>new ThriveaClient("tk_live_…")</c>, then <c>client.JobOpenings.ListAsync()</c>.
///     Thread-safe; create one and share it, as you would an <see cref="System.Net.Http.HttpClient" />.
/// </summary>
public sealed class ThriveaClient : IThriveaClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    /// <summary>A client for the production API with the given key.</summary>
    public ThriveaClient(string apiKey)
        : this(new ThriveaClientOptions { ApiKey = apiKey })
    {
    }

    /// <summary>A client configured by <paramref name="options" />.</summary>
    public ThriveaClient(ThriveaClientOptions options)
        : this(options?.HttpClient ?? CreateHttpClient(), options ?? throw new ArgumentNullException(nameof(options)), ownsHttpClient: options.HttpClient is null)
    {
    }

    /// <summary>
    ///     A client that sends with <paramref name="httpClient" />, the shape <c>IHttpClientFactory</c> typed clients need:
    ///     <c>services.AddHttpClient&lt;IThriveaClient, ThriveaClient&gt;()</c> with the options registered as a singleton.
    /// </summary>
    public ThriveaClient(HttpClient httpClient, ThriveaClientOptions options)
        : this(httpClient, options, ownsHttpClient: false)
    {
    }

    private ThriveaClient(HttpClient httpClient, ThriveaClientOptions options, bool ownsHttpClient)
    {
        if (httpClient is null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Validate();

        _httpClient = httpClient;
        _ownsHttpClient = ownsHttpClient;
        Options = options;

        var transport = new ThriveaHttpTransport(httpClient, options);
        JobOpenings = new JobOpeningService(transport);
        CandidateApplications = new CandidateApplicationService(transport);
        Candidates = new CandidateService(transport);
        Employees = new EmployeeService(transport);
        OrgUnits = new OrgUnitService(transport);
        OrgUnitTypes = new OrgUnitTypeService(transport);
        CostCenters = new CostCenterService(transport);
        WebhookEndpoints = new WebhookEndpointService(transport);
    }

    /// <summary>The options this client was built with.</summary>
    public ThriveaClientOptions Options { get; }

    /// <inheritdoc />
    public IJobOpeningService JobOpenings { get; }

    /// <inheritdoc />
    public ICandidateApplicationService CandidateApplications { get; }

    /// <inheritdoc />
    public ICandidateService Candidates { get; }

    /// <inheritdoc />
    public IEmployeeService Employees { get; }

    /// <inheritdoc />
    public IOrgUnitService OrgUnits { get; }

    /// <inheritdoc />
    public IOrgUnitTypeService OrgUnitTypes { get; }

    /// <inheritdoc />
    public ICostCenterService CostCenters { get; }

    /// <inheritdoc />
    public IWebhookEndpointService WebhookEndpoints { get; }

    /// <summary>Disposes the <see cref="System.Net.Http.HttpClient" /> only when this client created it.</summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private static HttpClient CreateHttpClient()
    {
        // The per-attempt timeout is enforced by the transport, so the HttpClient's own stays out of the way.
        return new HttpClient { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
    }
}
