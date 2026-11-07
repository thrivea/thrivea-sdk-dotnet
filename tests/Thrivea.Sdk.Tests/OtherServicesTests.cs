using System.Net;
using System.Text.Json;

namespace Thrivea.Sdk.Tests;

public class OtherServicesTests
{
    [Fact]
    public async Task Employees_read_the_legacy_integer_status_and_tolerate_a_name()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"object":"list","url":"/v1/employees","data":[{"id":"emp_00000000000000000000000001","object":"employee","first_name":"Ana","last_name":"Anić","status":2},{"id":"emp_00000000000000000000000002","object":"employee","first_name":"Bo","last_name":"Bošković","status":"pending_deactivation"},{"id":"emp_00000000000000000000000003","object":"employee","first_name":"C","last_name":"D","status":99}],"has_more":false}""");

        var page = await client.Employees.ListAsync(new EmployeeListOptions { Limit = 100 });

        Assert.Equal("https://api.example.test/v1/employees?limit=100", handler.Requests[0].Url);
        Assert.Equal(EmployeeStatus.Active, page.Data[0].Status);
        Assert.Equal(EmployeeStatus.PendingDeactivation, page.Data[1].Status);
        Assert.Equal((EmployeeStatus)99, page.Data[2].Status);
        Assert.Null(page.Data[0].BirthDate);
    }

    [Fact]
    public async Task Candidates_list_by_email_and_read_their_tags_by_name()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"object":"list","url":"/v1/candidates","data":[{"object":"candidate","id":"cand_00000000000000000000000001","first_name":"Ana","last_name":"Anić","email":"ana@example.com","phone":null,"headline":null,"tags":["#frontend-engineer"],"created_at":"2026-01-01T00:00:00Z","updated_at":"2026-01-02T00:00:00Z"}],"has_more":false}""");
        handler.Enqueue(HttpStatusCode.OK, """{"object":"candidate","id":"cand_00000000000000000000000001","first_name":"Ana","last_name":"Anić","email":"ana@example.com","tags":[],"created_at":"2026-01-01T00:00:00Z","updated_at":"2026-01-01T00:00:00Z"}""");

        var page = await client.Candidates.ListAsync(new CandidateListOptions { Email = "ana@example.com", Limit = 1 });
        var candidate = await client.Candidates.GetAsync("cand_00000000000000000000000001");

        Assert.Equal("https://api.example.test/v1/candidates?limit=1&email=ana%40example.com", handler.Requests[0].Url);
        Assert.Equal("#frontend-engineer", Assert.Single(page.Data[0].Tags));
        Assert.Null(page.Data[0].Phone);
        Assert.Equal("https://api.example.test/v1/candidates/cand_00000000000000000000000001", handler.Requests[1].Url);
        Assert.Equal("Anić", candidate.LastName);
        await Assert.ThrowsAsync<ArgumentException>(() => client.Candidates.GetAsync("job_00000000000000000000000001"));
    }

    [Fact]
    public async Task Cost_centers_send_updated_after_as_an_iso_instant()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"object":"list","data":[{"object":"cost_center","id":"cc_00000000000000000000000001","code":"CC-1","job_position_ids":["pos_00000000000000000000000001"],"active":true,"created_at":"2026-01-01T00:00:00Z","updated_at":"2026-02-01T00:00:00Z"}],"has_more":false}""");

        var page = await client.CostCenters.ListAsync(new CostCenterListOptions { UpdatedAfter = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.FromHours(1)) });

        Assert.Equal("https://api.example.test/v1/cost_centers?updated_after=2026-01-15T11%3A00%3A00.0000000Z", handler.Requests[0].Url);
        Assert.Equal("pos_00000000000000000000000001", Assert.Single(page.Data[0].JobPositionIds));
    }

    [Fact]
    public async Task Update_is_a_patch_with_only_the_fields_given()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"object":"org_unit_type","id":"orgut_00000000000000000000000001","name":"Team","hex_color":"#112233","status":2,"created_at":"2026-01-01T00:00:00Z","updated_at":"2026-01-02T00:00:00Z"}""");

        var type = await client.OrgUnitTypes.UpdateAsync("orgut_00000000000000000000000001", new OrgUnitTypeUpdateOptions { Status = OrgUnitTypeStatus.Archived });

        var request = Assert.Single(handler.Requests);
        Assert.Equal("PATCH", request.Method);
        Assert.Equal("https://api.example.test/v1/org_unit_types/orgut_00000000000000000000000001", request.Url);
        Assert.Equal("""{"status":2}""", request.Body);
        Assert.Equal(OrgUnitTypeStatus.Archived, type.Status);
    }

    [Fact]
    public async Task Delete_returns_the_deletion_stub()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"id":"orgu_00000000000000000000000001","object":"org_unit","deleted":true}""");

        var deleted = await client.OrgUnits.DeleteAsync("orgu_00000000000000000000000001");

        Assert.Equal("DELETE", handler.Requests[0].Method);
        Assert.True(deleted.Deleted);
        Assert.Equal("orgu_00000000000000000000000001", deleted.Id);
    }

    [Fact]
    public async Task Org_unit_criteria_keep_every_field_the_api_sends()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.OK, """{"id":"orgu_00000000000000000000000001","object":"org_unit","name":"Engineering","org_unit_type_ids":["orgut_00000000000000000000000001"],"parent_org_unit_ids":[],"sibling_org_unit_ids":[],"managers_criteria":{"includes":[{"type":"ByEmployeeIds","employeeIds":["emp_00000000000000000000000001"]}],"excludes":[]},"employees_criteria":{"includes":[{"type":"Everyone"}],"excludes":[{"type":"None"}]},"status":1,"created_at":"2026-01-01T00:00:00Z","created_by":"emp_00000000000000000000000001","updated_at":"2026-01-01T00:00:00Z","updated_by":"emp_00000000000000000000000001","external_code":""}""");

        var unit = await client.OrgUnits.GetAsync("orgu_00000000000000000000000001");

        Assert.Equal(OrgUnitStatus.Active, unit.Status);
        var managers = Assert.Single(unit.ManagersCriteria!.Includes);
        Assert.Equal(PeopleCriterionTypes.ByEmployeeIds, managers.Type);
        Assert.Equal("emp_00000000000000000000000001", managers.Properties!["employeeIds"][0].GetString());
        Assert.Equal(PeopleCriterionTypes.Everyone, Assert.Single(unit.EmployeesCriteria!.Includes).Type);

        // And they go back out as they came in.
        var json = JsonSerializer.Serialize(unit.ManagersCriteria, Internal.ThriveaJson.Options);
        Assert.Contains("\"employeeIds\":[\"emp_00000000000000000000000001\"]", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Creating_a_webhook_endpoint_captures_the_one_time_secret_header()
    {
        var (client, handler) = TestClient.Create();
        handler.Enqueue(HttpStatusCode.Created, """{"id":"we_00000000000000000000000001","object":"webhook_endpoint","url":"https://example.com/hook","enabled":true,"event_types":["employee.created"],"created_at":"2026-01-01T00:00:00Z"}""", ("Thrivea-Webhook-Secret", "whsec_abc"));
        handler.Enqueue(HttpStatusCode.OK, """{"object":"webhook_event_types","data":["employee.created","employee.updated"]}""");

        var endpoint = await client.WebhookEndpoints.CreateAsync(new WebhookEndpointCreateOptions { Url = "https://example.com/hook", EventTypes = { WebhookEventTypes.EmployeeCreated } });
        var types = await client.WebhookEndpoints.ListEventTypesAsync();

        Assert.Equal("whsec_abc", endpoint.Secret);
        Assert.Equal("employee.created", Assert.Single(endpoint.EventTypes));
        using var body = JsonDocument.Parse(handler.Requests[0].Body!);
        Assert.True(body.RootElement.GetProperty("enabled").GetBoolean());
        Assert.Equal(["employee.created", "employee.updated"], types);
        Assert.Equal("https://api.example.test/v1/webhook_endpoints/event_types", handler.Requests[1].Url);
    }
}
