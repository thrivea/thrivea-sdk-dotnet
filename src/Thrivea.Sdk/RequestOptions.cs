namespace Thrivea;

/// <summary>Per-request settings that override the client's.</summary>
public sealed class RequestOptions
{
    /// <summary>
    ///     Sent as the <c>Idempotency-Key</c> header. Give a <c>POST</c> one (a GUID you store with the thing you are
    ///     creating) and the SDK may safely retry it after a network failure. The ATS resources are also idempotent by
    ///     their own <c>external_ref</c>, so a retried create never makes a second job opening or application.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>An API key to use for this request instead of the client's.</summary>
    public string? ApiKey { get; set; }
}
