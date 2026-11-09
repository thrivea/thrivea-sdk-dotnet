using Thrivea.Ids;
using Thrivea.Webhooks;

namespace Thrivea.Sdk.Tests;

public class WebhookTests
{
    private const string Secret = "whsec_test_secret";
    private const string Payload = """{"id":"0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5b","created":"2026-09-29T10:15:30.1234567+00:00","type":"org_unit_type.created","data":{"OrgUnitTypeId":"0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5c","Name":"Department","HexColor":"#4F46E5"}}""";

    [Fact]
    public void Compute_matches_the_documented_algorithm()
    {
        // HMAC-SHA256("1700000000.{payload}", secret), lowercase hex, as Thrivea's own example computes it.
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(Secret));
        var expected = Convert.ToHexString(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("1700000000." + Payload))).ToLowerInvariant();

        Assert.Equal(expected, WebhookSignature.Compute(Payload, "1700000000", Secret));
    }

    [Fact]
    public void A_fresh_correctly_signed_request_verifies()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1700000000);
        var signature = WebhookSignature.Compute(Payload, "1700000000", Secret);

        WebhookSignature.Verify(Payload, signature, "1700000000", Secret, now: now.AddMinutes(2));
        Assert.True(WebhookSignature.IsValid(Payload, signature.ToUpperInvariant(), "1700000000", Secret, now: now));
    }

    [Fact]
    public void A_tampered_body_a_wrong_secret_or_a_stale_timestamp_is_refused()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1700000000);
        var signature = WebhookSignature.Compute(Payload, "1700000000", Secret);

        Assert.Throws<ThriveaWebhookSignatureException>(() => WebhookSignature.Verify(Payload + " ", signature, "1700000000", Secret, now: now));
        Assert.Throws<ThriveaWebhookSignatureException>(() => WebhookSignature.Verify(Payload, signature, "1700000000", "other", now: now));
        Assert.Throws<ThriveaWebhookSignatureException>(() => WebhookSignature.Verify(Payload, signature, "1700000000", Secret, now: now.AddMinutes(6)));
        Assert.Throws<ThriveaWebhookSignatureException>(() => WebhookSignature.Verify(Payload, null, "1700000000", Secret, now: now));
        Assert.Throws<ThriveaWebhookSignatureException>(() => WebhookSignature.Verify(Payload, signature, "yesterday", Secret, now: now));
        WebhookSignature.Verify(Payload, signature, "1700000000", Secret, tolerance: TimeSpan.Zero, now: now.AddDays(3));
    }

    [Fact]
    public void An_employee_created_event_reads_into_EmployeeCreatedData()
    {
        var body = """{"id":"0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5b","created":"2026-11-08T10:15:30.1234567+00:00","type":"employee.created","data":{"EmployeeId":"0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5c","EmailAddress":"ana.anic@example.com"}}""";

        var data = WebhookEvent.Parse(body).GetData<EmployeeCreatedData>();

        Assert.Equal(Guid.Parse("0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5c"), data!.EmployeeId);
        Assert.Equal("ana.anic@example.com", data.EmailAddress);
        Assert.Equal("emp", TypeId.FromUuid(ResourcePrefixes.Employee, data.EmployeeId).Prefix);
    }

    [Fact]
    public void A_job_opening_event_reads_into_the_JobOpening_the_API_returns()
    {
        var body = """{"id":"0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5b","created":"2026-11-09T10:15:30.1234567+00:00","type":"job_opening.closed","data":{"object":"job_opening","id":"job_01h2xcejqtf2nbrexx3vqjhp41","title":"Senior .NET Engineer","status":"closed","workplace_type":"hybrid","published":true,"external_ref":"vega-1001","created_at":"2026-11-01T08:00:00+00:00"}}""";

        var webhookEvent = WebhookEvent.Parse(body);
        var job = webhookEvent.GetData<JobOpening>();

        Assert.Equal(WebhookEventTypes.JobOpeningClosed, webhookEvent.Type);
        Assert.Equal("job_01h2xcejqtf2nbrexx3vqjhp41", job!.Id);
        Assert.Equal(JobOpeningStatus.Closed, job.Status);
        Assert.Equal(Thrivea.WorkplaceType.Hybrid, job.WorkplaceType);
        Assert.True(job.Published);
        Assert.Equal("vega-1001", job.ExternalRef);
    }

    [Fact]
    public void The_event_parses_and_its_data_deserializes_case_insensitively()
    {
        var signature = WebhookSignature.Compute(Payload, "1700000000", Secret);

        var webhookEvent = WebhookEvent.ParseAndVerify(Payload, signature, "1700000000", Secret, tolerance: TimeSpan.Zero);
        var data = webhookEvent.GetData<OrgUnitTypeCreatedData>();

        Assert.Equal("org_unit_type.created", webhookEvent.Type);
        Assert.Equal("0199a1b2-c3d4-7e5f-8a9b-0c1d2e3f4a5b", webhookEvent.Id);
        Assert.Equal(new DateTimeOffset(2026, 9, 29, 10, 15, 30, TimeSpan.Zero).AddTicks(1234567), webhookEvent.Created);
        Assert.Equal("Department", data!.Name);
        Assert.Equal("#4F46E5", data.HexColor);
    }

    private sealed class OrgUnitTypeCreatedData
    {
        public string? Name { get; init; }

        public string? HexColor { get; init; }
    }
}
