# Thrivea SDK for .NET

[![NuGet](https://img.shields.io/nuget/v/Thrivea.Sdk.svg)](https://www.nuget.org/packages/Thrivea.Sdk)
[![CI](https://github.com/thrivea/thrivea-sdk-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/thrivea/thrivea-sdk-dotnet/actions/workflows/ci.yml)

The official .NET client for the [Thrivea](https://thrivea.com) public REST API (`/v1`). It gives your own systems, a
careers website, an HRIS sync, an internal tool, the same job openings, candidates, employees, org units, cost centers
and webhooks that Thrivea's screens use.

- One service per resource, Stripe-style: `client.JobOpenings.ListAsync()`, `client.CandidateApplications.CreateAsync(…)`.
- Prefixed identifiers (`job_01h2xcejqtf2nbrexx3vqjhp41`) checked before a request leaves, with `TypeId` to parse them.
- Typed exceptions per API error, retries with backoff for what is safe to retry, cursor paging with auto-paging.
- Targets `netstandard2.0` and `net8.0`: .NET Framework 4.6.2+, .NET 6/8/10, Umbraco, ASP.NET Core, console apps.

## Install

```shell
dotnet add package Thrivea.Sdk
```

## Quickstart

```csharp
using Thrivea;

var client = new ThriveaClient("tk_live_…");

// The jobs you are hiring for.
await foreach (var job in client.JobOpenings.ListAutoPagingAsync())
{
    Console.WriteLine($"{job.Id} {job.Title} ({job.Status})");
}

// An application from your own careers form, CV included.
var application = await client.CandidateApplications.CreateAsync(new CandidateApplicationCreateOptions
{
    JobOpeningId = "job_01h2xcejqtf2nbrexx3vqjhp41",
    FirstName = "Ana",
    LastName = "Anić",
    Email = "ana@example.com",
    Phone = "+381 60 000 0000",
    CoverNote = "Preferred city: Novi Sad. Heard about us: LinkedIn.",
    Tags = { "#frontend-engineer", "#novi-sad" },
    ExternalRef = "form-submission-8831",
    Cv = FileUpload.FromPath("ana-cv.pdf")
});

Console.WriteLine($"Application {application.Id} for candidate {application.CandidateId}");

// The person behind it, with the tags they carry.
var candidate = await client.Candidates.GetAsync(application.CandidateId);
```

## Authentication

Every request carries `Authorization: Bearer <api key>`. An API key is minted in Thrivea with the **scopes** the
integration needs and acts as the person who minted it. A careers website needs `read:job_openings`,
`write:job_openings` and `write:candidate_applications`; give it nothing more, and the key cannot read the employee
directory even if it leaks.

| You get | Because |
|---|---|
| `ThriveaAuthenticationException` (401) | The key is missing, wrong, expired or revoked. |
| `ThriveaPermissionException` (403) | The key lacks the scope, the person it acts for lacks the permission, or the workspace's plan lacks the module. The message says which. |

## A careers website, end to end

The pattern Thrivea built the ATS resources for: your CMS is the author of the job ad, Thrivea is the applicant
tracking system behind it.

```csharp
// 1. When an ad is written, find the Thrivea job it belongs to or create one, and store the id on the ad.
//    Several ads can point at one job. `external_ref` makes the create idempotent: the same ref again
//    returns the job already created, never a second one.
var job = await client.JobOpenings.CreateAsync(new JobOpeningCreateOptions
{
    Title = "Frontend Engineer",
    WorkplaceType = WorkplaceType.Onsite,
    ExternalRef = "cms-ad-42",
    Description = "Summary of the role. Full ad: https://www.example.com/career/frontend-engineer"
});
// job.Id is "job_…"; it is open, not published on Thrivea's careers page, and uses no posting slot.

// 2. When the form is submitted, send the application with the ad's hashtag.
var application = await client.CandidateApplications.CreateAsync(new CandidateApplicationCreateOptions
{
    JobOpeningId = job.Id,
    FirstName = form.FirstName,
    LastName = form.LastName,
    Email = form.Email,
    CoverNote = form.Comment,
    Tags = { "#frontend-engineer", $"#{form.City.ToLowerInvariant()}" },
    ExternalRef = form.SubmissionId,
    Cv = form.CvStream is null ? null : new FileUpload(form.CvStream, form.CvFileName, FileUpload.ContentTypeFor(form.CvFileName))
});
```

What the API does with it: the candidate is found by e-mail or created; the tags are found by name (the `#` is
optional) or created and added to the candidate; the CV is stored; the application lands in the job's first pipeline
stage; the hiring team is told. The same candidate applying to the same job again returns the first application, so a
form that retries makes one.

## Identifiers

Every id is a prefix, an underscore and 26 characters: `emp_…`, `job_…`, `cand_…`, `capp_…`. Store them as strings.
The services check the prefix before sending, so passing a candidate id where a job id belongs fails at once with an
`ArgumentException` naming the parameter, not as a `400` from the API.

```csharp
using Thrivea.Ids;

var id = TypeId.Parse("job_01h2xcejqtf2nbrexx3vqjhp41");
id.Prefix;                       // "job"
id.Uuid;                         // the UUID it encodes
TypeId.IsValid(value, ResourcePrefixes.JobOpening);
```

## Errors

```csharp
using Thrivea.Errors;

try
{
    await client.JobOpenings.GetAsync(id);
}
catch (ThriveaNotFoundException)
{
    // No such job in this workspace.
}
catch (ThriveaInvalidRequestException exception)
{
    // exception.Error.Param names the field; exception.Error.Message says what is wrong with it.
}
catch (ThriveaException exception)
{
    // Everything else the API can say: exception.StatusCode, exception.Error.Type, exception.RequestId.
}
```

| Exception | Status | `Error.Type` |
|---|---|---|
| `ThriveaInvalidRequestException` | 400 | `invalid_request_error` |
| `ThriveaAuthenticationException` | 401 | `authentication_error` |
| `ThriveaPermissionException` | 403 | `permission_error` |
| `ThriveaNotFoundException` | 404 | `resource_missing` |
| `ThriveaConflictException` | 409 | `conflict_error` |
| `ThriveaRateLimitException` | 429 | `rate_limit_error`, with `RetryAfter` |
| `ThriveaApiException` | 5xx | `api_error` |
| `ThriveaNetworkException` | none | the request got no answer |

Branch on the exception type or on `Error.Type`, never on the message: messages are for people and logs.

## Retries and idempotency

Reads and deletes are retried after a network failure, a `409`, a `429` or a `5xx`, up to
`ThriveaClientOptions.MaxNetworkRetries` times (default 2), with exponential backoff and never sooner than the API's
`Retry-After`. A `POST` or `PATCH` is retried only when you give it an idempotency key:

```csharp
await client.CandidateApplications.CreateAsync(options, new RequestOptions { IdempotencyKey = form.SubmissionId });
```

The ATS resources are also idempotent by their own `external_ref` and by candidate and job, so a retried create never
makes a second job opening or application.

## Paging

Lists come one page at a time, with `has_more` and a cursor:

```csharp
var page = await client.Employees.ListAsync(new EmployeeListOptions { Limit = 100 });
while (page.HasMore)
{
    page = await client.Employees.ListAsync(new EmployeeListOptions { Limit = 100, StartingAfter = page.Data[^1].Id });
}
```

Or let the SDK walk them:

```csharp
await foreach (var employee in client.Employees.ListAutoPagingAsync())
{
    …
}
```

Cost centers take `UpdatedAfter` for an incremental sync.

## Webhooks

Create an endpoint and keep the secret the response gives you once:

```csharp
var endpoint = await client.WebhookEndpoints.CreateAsync(new WebhookEndpointCreateOptions
{
    Url = "https://www.example.com/thrivea/webhook",
    EventTypes = { WebhookEventTypes.EmployeeCreated, WebhookEventTypes.EmployeeUpdated }
});
Store(endpoint.Secret); // shown this once
```

Verify each delivery before trusting it:

```csharp
using Thrivea.Webhooks;

var body = await new StreamReader(Request.Body).ReadToEndAsync();
var webhookEvent = WebhookEvent.ParseAndVerify(
    body,
    Request.Headers[WebhookSignature.SignatureHeader],
    Request.Headers[WebhookSignature.TimestampHeader],
    secret);

if (webhookEvent.Type == WebhookEventTypes.EmployeeCreated)
{
    var data = webhookEvent.GetData<EmployeeCreatedData>();
}
```

Answer `2xx` within 10 seconds and do the work afterwards; deliveries are at least once, so ignore an `Id` you have
already processed.

## Dependency injection (ASP.NET Core, Umbraco)

```csharp
builder.Services.AddSingleton(new ThriveaClientOptions { ApiKey = builder.Configuration["Thrivea:ApiKey"]! });
builder.Services.AddHttpClient<IThriveaClient, ThriveaClient>();
```

Then take `IThriveaClient` in a constructor. The `HttpClient` comes from `IHttpClientFactory`, so handlers, logging
and lifetime are the host's.

## Configuration

```csharp
var client = new ThriveaClient(new ThriveaClientOptions
{
    ApiKey = "tk_live_…",
    BaseUrl = "https://api.thrivea.com", // the default; a Thrivea engineer can give you a test host
    MaxNetworkRetries = 2,
    Timeout = TimeSpan.FromSeconds(30)
});
```

## Compatibility

The API only adds: a new field, a new resource or a new enum value never breaks a client. The SDK follows suit.
An enum value this version does not know comes through as the string the API sent (job openings) or as the enum's
`Unknown` member (employees, org units), never as an exception. The package follows semantic versioning; the major
version changes only with the API's.

## Resources covered

| Service | Endpoints |
|---|---|
| `JobOpenings` | list, get, create |
| `CandidateApplications` | create (JSON, or multipart with a CV) |
| `Candidates` | list (by e-mail, incremental), get |
| `Employees` | list, get |
| `OrgUnits` | list, get, create, update, delete |
| `OrgUnitTypes` | list, get, create, update, delete |
| `CostCenters` | list, get, create, update |
| `WebhookEndpoints` | list, get, create, update, delete, event types |

## Development

```shell
dotnet build Thrivea.Sdk.slnx
dotnet test Thrivea.Sdk.slnx
dotnet run --project samples/Thrivea.Sdk.Samples.Careers -- list   # needs THRIVEA_API_KEY
```

## License

[MIT](LICENSE). Copyright (c) 2026 Thrivea d.o.o.
