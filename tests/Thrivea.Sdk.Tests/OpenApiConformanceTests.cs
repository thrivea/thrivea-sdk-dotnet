using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Thrivea.Errors;
using Thrivea.Services;

namespace Thrivea.Sdk.Tests;

/// <summary>
///     The SDK against the API's own contract: the OpenAPI document vendored under <c>openapi/</c> (exported from the
///     backend; see <c>openapi/README.md</c>). The SDK duplicates the wire models on purpose, so this is what keeps the two
///     copies honest: a route the SDK calls must exist, every field it reads or writes must be in the schema, every field
///     the schema has must be known to the SDK, and the cross-cutting headers must be declared.
/// </summary>
public class OpenApiConformanceTests
{
    private static readonly Lazy<JsonDocument> Document = new(Load);

    private static JsonElement Paths => Document.Value.RootElement.GetProperty("paths");

    private static JsonElement Schemas => Document.Value.RootElement.GetProperty("components").GetProperty("schemas");

    /// <summary>Every route a service calls, as (method, path). A route missing here is a route the SDK calls into a 404.</summary>
    public static IEnumerable<object[]> Routes =>
    [
        ["GET", "/v1/job_openings"], ["GET", "/v1/job_openings/{id}"], ["POST", "/v1/job_openings"],
        ["POST", "/v1/candidate_applications"],
        ["GET", "/v1/candidates"], ["GET", "/v1/candidates/{id}"],
        ["GET", "/v1/employees"], ["GET", "/v1/employees/{id}"],
        ["GET", "/v1/org_units"], ["GET", "/v1/org_units/{id}"], ["POST", "/v1/org_units"], ["PATCH", "/v1/org_units/{id}"], ["DELETE", "/v1/org_units/{id}"],
        ["GET", "/v1/org_unit_types"], ["GET", "/v1/org_unit_types/{id}"], ["POST", "/v1/org_unit_types"], ["PATCH", "/v1/org_unit_types/{id}"], ["DELETE", "/v1/org_unit_types/{id}"],
        ["GET", "/v1/cost_centers"], ["GET", "/v1/cost_centers/{id}"], ["POST", "/v1/cost_centers"], ["PATCH", "/v1/cost_centers/{id}"],
        ["GET", "/v1/webhook_endpoints"], ["GET", "/v1/webhook_endpoints/{id}"], ["GET", "/v1/webhook_endpoints/event_types"],
        ["POST", "/v1/webhook_endpoints"], ["PATCH", "/v1/webhook_endpoints/{id}"], ["DELETE", "/v1/webhook_endpoints/{id}"]
    ];

    /// <summary>Each SDK wire type and the schema it must match, field for field.</summary>
    public static IEnumerable<object[]> Models =>
    [
        [typeof(JobOpening), "JobOpening"], [typeof(JobOpeningCreateOptions), "CreateJobOpeningBody"],
        [typeof(CandidateApplication), "CandidateApplication"], [typeof(CandidateApplicationCreateOptions), "CandidateApplicationBody"],
        [typeof(Candidate), "Candidate"],
        [typeof(Employee), "Employee"],
        [typeof(OrgUnit), "OrgUnit"], [typeof(OrgUnitCreateOptions), "CreateOrgUnitRequest"], [typeof(OrgUnitUpdateOptions), "UpdateOrgUnitRequest"],
        [typeof(PeopleCriteria), "PeopleCriteriaResource"], [typeof(PeopleCriterion), "PeopleCriterionResource"],
        [typeof(OrgUnitType), "OrgUnitType"], [typeof(OrgUnitTypeCreateOptions), "CreateOrgUnitTypeRequest"], [typeof(OrgUnitTypeUpdateOptions), "UpdateOrgUnitTypeRequest"],
        [typeof(CostCenter), "CostCenter"], [typeof(CostCenterCreateOptions), "CreateCostCenterRequest"], [typeof(CostCenterUpdateOptions), "UpdateCostCenterRequest"],
        [typeof(WebhookEndpoint), "WebhookEndpoint"], [typeof(WebhookEndpointCreateOptions), "CreateWebhookEndpointRequest"], [typeof(WebhookEndpointUpdateOptions), "UpdateWebhookEndpointRequest"],
        [typeof(ThriveaError), "ErrorDetails"], [typeof(DeletedResource), "DeletedResponse"],
        [typeof(ThriveaList<JobOpening>), "JobOpeningListResponse"], [typeof(ThriveaList<Candidate>), "CandidateListResponse"], [typeof(ThriveaList<Employee>), "EmployeeListResponse"],
        [typeof(ThriveaList<OrgUnit>), "OrgUnitListResponse"], [typeof(ThriveaList<OrgUnitType>), "OrgUnitTypeListResponse"], [typeof(ThriveaList<CostCenter>), "CostCenterListResponse"],
        [typeof(ThriveaList<WebhookEndpoint>), "WebhookEndpointListResponse"]
    ];

    /// <summary>Schema fields the SDK knowingly does not surface yet. Empty is the goal; an entry here is a to-do with a reason.</summary>
    private static readonly Dictionary<string, string[]> NotYetSurfaced = new();

    [Theory]
    [MemberData(nameof(Routes))]
    public void Every_route_the_sdk_calls_is_in_the_document(string method, string path)
    {
        Assert.True(Paths.TryGetProperty(path, out var operations), $"{path} is not in the document");
        Assert.True(operations.TryGetProperty(method.ToLowerInvariant(), out _), $"{method} {path} is not in the document");
    }

    [Fact]
    public void Every_route_in_the_document_is_one_the_sdk_calls()
    {
        var known = Routes.Select(route => $"{route[0]} {route[1]}").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknown = Paths.EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject().Select(op => $"{op.Name.ToUpperInvariant()} {path.Name}"))
            .Where(route => !known.Contains(route))
            .ToList();

        Assert.True(unknown.Count == 0, "The API has routes the SDK does not call yet: " + string.Join(", ", unknown));
    }

    [Theory]
    [MemberData(nameof(Models))]
    public void Every_field_the_sdk_reads_or_writes_is_in_the_schema(Type sdkType, string schemaName)
    {
        var schema = SchemaProperties(schemaName);
        var missing = WireNames(sdkType).Where(name => !schema.Contains(name)).ToList();

        Assert.True(missing.Count == 0, $"{sdkType.Name} has fields the schema {schemaName} does not: {string.Join(", ", missing)}");
    }

    [Theory]
    [MemberData(nameof(Models))]
    public void Every_field_in_the_schema_is_known_to_the_sdk(Type sdkType, string schemaName)
    {
        var known = WireNames(sdkType).ToHashSet(StringComparer.Ordinal);
        var allowed = NotYetSurfaced.TryGetValue(schemaName, out var names) ? names : [];
        var unknown = SchemaProperties(schemaName).Where(name => !known.Contains(name) && !allowed.Contains(name)).ToList();

        Assert.True(unknown.Count == 0, $"The schema {schemaName} has fields {sdkType.Name} does not surface: {string.Join(", ", unknown)}");
    }

    [Fact]
    public void Every_write_takes_an_idempotency_key_and_every_answer_carries_request_id_and_rate_limit_headers()
    {
        var problems = new List<string>();
        foreach (var path in Paths.EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject())
            {
                var route = $"{operation.Name.ToUpperInvariant()} {path.Name}";
                var isWrite = operation.Name is "post" or "patch" or "put";
                if (isWrite)
                {
                    var hasKey = operation.Value.TryGetProperty("parameters", out var parameters)
                        && parameters.EnumerateArray().Any(parameter => parameter.GetProperty("in").GetString() == "header" && parameter.GetProperty("name").GetString() == "Idempotency-Key");
                    if (!hasKey)
                    {
                        problems.Add($"{route} does not declare the Idempotency-Key header");
                    }
                }

                foreach (var response in operation.Value.GetProperty("responses").EnumerateObject())
                {
                    var headers = response.Value.TryGetProperty("headers", out var declared) ? declared.EnumerateObject().Select(header => header.Name).ToHashSet() : [];
                    foreach (var expected in new[] { "Request-Id", "RateLimit-Limit", "RateLimit-Remaining", "RateLimit-Reset", "RateLimit-Policy" })
                    {
                        if (!headers.Contains(expected))
                        {
                            problems.Add($"{route} {response.Name} does not declare {expected}");
                        }
                    }

                    if (isWrite && response.Name.StartsWith('2') && !headers.Contains("Idempotent-Replayed"))
                    {
                        problems.Add($"{route} {response.Name} does not declare Idempotent-Replayed");
                    }
                }
            }
        }

        Assert.True(problems.Count == 0, string.Join("\n", problems));
    }

    [Theory]
    [InlineData(typeof(EmployeeStatus), "EmployeeStatus")]
    [InlineData(typeof(OrgUnitStatus), "OrgUnitStatus")]
    [InlineData(typeof(OrgUnitTypeStatus), "OrgUnitTypeStatus")]
    public void Every_value_of_a_legacy_enum_is_in_the_schema(Type enumType, string schemaName)
    {
        var schema = Schemas.GetProperty(schemaName);
        var declared = schema.GetProperty("enum").EnumerateArray().Select(value => value.GetInt32()).ToHashSet();
        var sdkValues = Enum.GetValues(enumType).Cast<int>().Where(value => value != 0).ToList();

        var missing = sdkValues.Where(value => !declared.Contains(value)).ToList();
        var unknown = declared.Where(value => !sdkValues.Contains(value)).ToList();
        Assert.True(missing.Count == 0, $"{enumType.Name} has values the schema does not: {string.Join(", ", missing)}");
        Assert.True(unknown.Count == 0, $"The schema {schemaName} has values {enumType.Name} does not: {string.Join(", ", unknown)}");
    }

    [Fact]
    public void The_api_key_management_routes_are_not_part_of_the_public_contract()
    {
        Assert.DoesNotContain(Paths.EnumerateObject(), path => path.Name.StartsWith("/v1/api_keys", StringComparison.Ordinal));
    }

    private static HashSet<string> SchemaProperties(string schemaName)
    {
        Assert.True(Schemas.TryGetProperty(schemaName, out var schema), $"The document has no schema {schemaName}");
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (schema.TryGetProperty("properties", out var properties))
        {
            foreach (var property in properties.EnumerateObject())
            {
                names.Add(property.Name);
            }
        }

        if (schema.TryGetProperty("allOf", out var parts))
        {
            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("properties", out var more))
                {
                    foreach (var property in more.EnumerateObject())
                    {
                        names.Add(property.Name);
                    }
                }
            }
        }

        return names;
    }

    private static IReadOnlyList<string> WireNames(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            // A property ignored only when null is still on the wire; only an unconditional JsonIgnore hides one.
            .Where(property => property.GetCustomAttribute<JsonIgnoreAttribute>() is not { Condition: JsonIgnoreCondition.Always }
                && property.GetCustomAttribute<JsonExtensionDataAttribute>() is null)
            .Select(property => property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name)
            .ToList();
    }

    private static JsonDocument Load()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "openapi", "thrivea-v1.openapi.json");
            if (File.Exists(candidate))
            {
                return JsonDocument.Parse(File.ReadAllText(candidate));
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("openapi/thrivea-v1.openapi.json was not found above the test directory; see openapi/README.md.");
    }
}
