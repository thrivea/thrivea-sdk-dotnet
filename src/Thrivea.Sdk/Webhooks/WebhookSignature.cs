using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Thrivea.Errors;

namespace Thrivea.Webhooks;

/// <summary>
///     Checks that a webhook request came from Thrivea: the <c>Thrivea-Webhook-Signature</c> header is the lowercase hex
///     HMAC-SHA256 of <c>{timestamp}.{raw body}</c> with your endpoint's signing secret, and the
///     <c>Thrivea-Webhook-Timestamp</c> header (Unix seconds) must be close to now.
/// </summary>
public static class WebhookSignature
{
    /// <summary>The header carrying the signature.</summary>
    public const string SignatureHeader = "Thrivea-Webhook-Signature";

    /// <summary>The header carrying the Unix-seconds timestamp the signature covers.</summary>
    public const string TimestampHeader = "Thrivea-Webhook-Timestamp";

    /// <summary>How far from now a timestamp may be before the request is refused as a replay: 5 minutes.</summary>
    public static readonly TimeSpan DefaultTolerance = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Throws <see cref="ThriveaWebhookSignatureException" /> unless <paramref name="signature" /> is the signature of
    ///     <paramref name="payload" /> at <paramref name="timestamp" /> under <paramref name="secret" />, and the timestamp is
    ///     within <paramref name="tolerance" /> of now.
    /// </summary>
    /// <param name="payload">The raw request body, exactly as received.</param>
    /// <param name="signature">The <c>Thrivea-Webhook-Signature</c> header.</param>
    /// <param name="timestamp">The <c>Thrivea-Webhook-Timestamp</c> header.</param>
    /// <param name="secret">Your endpoint's signing secret.</param>
    /// <param name="tolerance">Defaults to <see cref="DefaultTolerance" />; <see cref="TimeSpan.Zero" /> skips the check.</param>
    /// <param name="now">The current time, for tests.</param>
    public static void Verify(string payload, string? signature, string? timestamp, string secret, TimeSpan? tolerance = null, DateTimeOffset? now = null)
    {
        if (payload is null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        if (string.IsNullOrEmpty(secret))
        {
            throw new ArgumentException("secret is required.", nameof(secret));
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new ThriveaWebhookSignatureException($"The {SignatureHeader} header is missing.");
        }

        if (string.IsNullOrWhiteSpace(timestamp) || !long.TryParse(timestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            throw new ThriveaWebhookSignatureException($"The {TimestampHeader} header is missing or is not Unix seconds.");
        }

        var window = tolerance ?? DefaultTolerance;
        if (window > TimeSpan.Zero)
        {
            var sent = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
            var drift = (now ?? DateTimeOffset.UtcNow) - sent;
            if (drift > window || drift < -window)
            {
                throw new ThriveaWebhookSignatureException($"The {TimestampHeader} header is {Math.Abs(drift.TotalSeconds):0} seconds from now; more than {window.TotalSeconds:0} is allowed.");
            }
        }

        var expected = Compute(payload, timestamp!.Trim(), secret);
        if (!FixedTimeEquals(expected, signature!.Trim().ToLowerInvariant()))
        {
            throw new ThriveaWebhookSignatureException("The signature does not match the payload: the request was not signed with this secret, or the body was altered.");
        }
    }

    /// <summary>Whether the signature is valid; the non-throwing form of <see cref="Verify" />.</summary>
    public static bool IsValid(string payload, string? signature, string? timestamp, string secret, TimeSpan? tolerance = null, DateTimeOffset? now = null)
    {
        try
        {
            Verify(payload, signature, timestamp, secret, tolerance, now);
            return true;
        }
        catch (ThriveaWebhookSignatureException)
        {
            return false;
        }
    }

    /// <summary>The signature Thrivea computes: lowercase hex HMAC-SHA256 of <c>{timestamp}.{payload}</c>.</summary>
    public static string Compute(string payload, string timestamp, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp + "." + payload));
        var chars = new char[hash.Length * 2];
        const string digits = "0123456789abcdef";
        for (var i = 0; i < hash.Length; i++)
        {
            chars[i * 2] = digits[hash[i] >> 4];
            chars[i * 2 + 1] = digits[hash[i] & 0xF];
        }

        return new string(chars);
    }

    // Compares every character, whatever the first difference, so timing says nothing about how much of a guess is right.
    private static bool FixedTimeEquals(string left, string right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        var difference = 0;
        for (var i = 0; i < left.Length; i++)
        {
            difference |= left[i] ^ right[i];
        }

        return difference == 0;
    }
}

/// <summary>A webhook request failed verification; see the message for which check.</summary>
public sealed class ThriveaWebhookSignatureException : ThriveaException
{
    /// <summary>Creates the exception.</summary>
    public ThriveaWebhookSignatureException(string message)
        : base(message)
    {
    }
}
