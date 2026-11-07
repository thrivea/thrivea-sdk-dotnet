using Thrivea.Ids;
using Thrivea.Internal;

namespace Thrivea.Services;

/// <summary><c>/v1/webhook_endpoints</c>.</summary>
public interface IWebhookEndpointService
{
    /// <summary>One page of webhook endpoints.</summary>
    Task<ThriveaList<WebhookEndpoint>> ListAsync(WebhookEndpointListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Every webhook endpoint, page after page.</summary>
    IAsyncEnumerable<WebhookEndpoint> ListAutoPagingAsync(WebhookEndpointListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>One webhook endpoint by id.</summary>
    Task<WebhookEndpoint> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a webhook endpoint. The result's <see cref="WebhookEndpoint.Secret" /> is shown this once: store it.</summary>
    Task<WebhookEndpoint> CreateAsync(WebhookEndpointCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Changes a webhook endpoint; see <see cref="WebhookEndpointUpdateOptions" />.</summary>
    Task<WebhookEndpoint> UpdateAsync(string id, WebhookEndpointUpdateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a webhook endpoint.</summary>
    Task<DeletedResource> DeleteAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);

    /// <summary>The event types an endpoint can subscribe to, as the API knows them today.</summary>
    Task<IReadOnlyList<string>> ListEventTypesAsync(RequestOptions? requestOptions = null, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IWebhookEndpointService" />
public sealed class WebhookEndpointService : ThriveaService, IWebhookEndpointService
{
    private const string BasePath = "/v1/webhook_endpoints";

    internal WebhookEndpointService(ThriveaHttpTransport transport)
        : base(transport)
    {
    }

    /// <inheritdoc />
    public Task<ThriveaList<WebhookEndpoint>> ListAsync(WebhookEndpointListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return Transport.GetAsync<ThriveaList<WebhookEndpoint>>(BasePath, (options ?? new WebhookEndpointListOptions()).ToQuery(), requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<WebhookEndpoint> ListAutoPagingAsync(WebhookEndpointListOptions? options = null, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        return AutoPageAsync<WebhookEndpoint, WebhookEndpointListOptions>(options, (page, token) => ListAsync(page, requestOptions, token), cancellationToken);
    }

    /// <inheritdoc />
    public Task<WebhookEndpoint> GetAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.WebhookEndpoint, nameof(id));
        return Transport.GetAsync<WebhookEndpoint>($"{BasePath}/{Uri.EscapeDataString(id)}", null, requestOptions, cancellationToken);
    }

    /// <summary>The response header that carries a new endpoint's signing secret, once.</summary>
    public const string SecretHeader = "Thrivea-Webhook-Secret";

    /// <inheritdoc />
    public async Task<WebhookEndpoint> CreateAsync(WebhookEndpointCreateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var response = await Transport.PostJsonWithResponseAsync<WebhookEndpoint>(BasePath, options, requestOptions, cancellationToken).ConfigureAwait(false);
        response.Value.Secret = response.Header(SecretHeader);
        return response.Value;
    }

    /// <inheritdoc />
    public Task<WebhookEndpoint> UpdateAsync(string id, WebhookEndpointUpdateOptions options, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.WebhookEndpoint, nameof(id));
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Transport.PatchJsonAsync<WebhookEndpoint>($"{BasePath}/{Uri.EscapeDataString(id)}", options, requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public Task<DeletedResource> DeleteAsync(string id, RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        TypeId.EnsurePrefix(id, ResourcePrefixes.WebhookEndpoint, nameof(id));
        return Transport.DeleteAsync<DeletedResource>($"{BasePath}/{Uri.EscapeDataString(id)}", requestOptions, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListEventTypesAsync(RequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        var envelope = await Transport.GetAsync<WebhookEventTypesEnvelope>($"{BasePath}/event_types", null, requestOptions, cancellationToken).ConfigureAwait(false);
        return envelope.Data;
    }
}
