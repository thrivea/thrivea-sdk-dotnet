# API reference

Every operation and every type of the Thrivea public API (`/v1`), generated from the OpenAPI document in
[`openapi/thrivea-v1.openapi.json`](https://github.com/thrivea/thrivea-sdk-dotnet/blob/main/openapi/thrivea-v1.openapi.json).
Do not edit it by hand: it is regenerated whenever the document changes. How to use each resource, with examples, is
on its own page; this one is the complete list.

Base URL `https://api.thrivea.com`. Every call sends `Authorization: Bearer <API key>`, and the key needs the scope the
operation names. Every answer carries `Request-Id` and the `RateLimit-*` headers; every write accepts `Idempotency-Key`.
Ids are TypeIDs: a prefix, an underscore and 26 characters, such as `job_01k9m3z4r7f8n9pq2s3t4v5w6x`.

## Operations

| Operation | What it does | Scope |
|---|---|---|
| [`POST /v1/candidate_applications`](#post-v1candidate_applications) | Submit an application, as JSON (no CV). | `write:candidate_applications` |
| [`GET /v1/candidates`](#get-v1candidates) | List candidates, oldest first. | `read:candidates` |
| [`GET /v1/candidates/{id}`](#get-v1candidatesid) | Retrieve a candidate. | `read:candidates` |
| [`GET /v1/cost_centers`](#get-v1cost_centers) | List cost centers, oldest first. | `read:cost_centers` |
| [`POST /v1/cost_centers`](#post-v1cost_centers) | Create a cost center. | `write:cost_centers` |
| [`GET /v1/cost_centers/{id}`](#get-v1cost_centersid) | Retrieve a cost center. | `read:cost_centers` |
| [`PATCH /v1/cost_centers/{id}`](#patch-v1cost_centersid) | Update a cost center. A field left out keeps its value. | `write:cost_centers` |
| [`GET /v1/employees`](#get-v1employees) | List all employees | `read:employees` |
| [`GET /v1/employees/{id}`](#get-v1employeesid) | Retrieve a specific employee | `read:employees` |
| [`GET /v1/job_openings`](#get-v1job_openings) | List job openings, oldest first. Open ones by default. | `read:job_openings` |
| [`POST /v1/job_openings`](#post-v1job_openings) | Create a job opening: open, not published, on the workspace's default pipeline, with no hiring team. The same external_ref again returns the job already created. | `write:job_openings` |
| [`GET /v1/job_openings/{id}`](#get-v1job_openingsid) | Retrieve a job opening. | `read:job_openings` |
| [`GET /v1/org_unit_types`](#get-v1org_unit_types) | List all org unit types | `read:org_unit_types` |
| [`POST /v1/org_unit_types`](#post-v1org_unit_types) | Create a new org unit type | `write:org_unit_types` |
| [`GET /v1/org_unit_types/{id}`](#get-v1org_unit_typesid) | Retrieve a specific org unit type | `read:org_unit_types` |
| [`PATCH /v1/org_unit_types/{id}`](#patch-v1org_unit_typesid) | Update an existing org unit type | `write:org_unit_types` |
| [`DELETE /v1/org_unit_types/{id}`](#delete-v1org_unit_typesid) | Delete an org unit type | `write:org_unit_types` |
| [`GET /v1/org_units`](#get-v1org_units) | List all org units | `read:org_units` |
| [`POST /v1/org_units`](#post-v1org_units) | Create a new org unit | `write:org_units` |
| [`GET /v1/org_units/{id}`](#get-v1org_unitsid) | Retrieve a specific org unit | `read:org_units` |
| [`PATCH /v1/org_units/{id}`](#patch-v1org_unitsid) | Update an existing org unit | `write:org_units` |
| [`DELETE /v1/org_units/{id}`](#delete-v1org_unitsid) | Delete an org unit | `write:org_units` |
| [`GET /v1/webhook_endpoints`](#get-v1webhook_endpoints) | List all webhook endpoints | `read:webhook_endpoints` |
| [`POST /v1/webhook_endpoints`](#post-v1webhook_endpoints) | Create a new webhook endpoint | `write:webhook_endpoints` |
| [`GET /v1/webhook_endpoints/event_types`](#get-v1webhook_endpointsevent_types) | List all available webhook event types | `read:webhook_endpoints` |
| [`GET /v1/webhook_endpoints/{id}`](#get-v1webhook_endpointsid) | Retrieve a specific webhook endpoint | `read:webhook_endpoints` |
| [`PATCH /v1/webhook_endpoints/{id}`](#patch-v1webhook_endpointsid) | Update an existing webhook endpoint | `write:webhook_endpoints` |
| [`DELETE /v1/webhook_endpoints/{id}`](#delete-v1webhook_endpointsid) | Delete a webhook endpoint | `write:webhook_endpoints` |

### POST /v1/candidate_applications

Submit an application, as JSON (no CV).

The same path also accepts multipart/form-data with these fields in snake_case (tags repeated or comma-separated) and the CV in an optional file field named cv. The candidate is found by email or created; an existing candidate keeps the name Thrivea holds. A first application to the job answers 201. The same candidate applying to the same job again, the next day or a year later, updates that one application and answers 200: a new CV becomes the current version and the earlier ones stay, a new cover note replaces the old, a new phone number goes onto the candidate, and an application that was rejected or declined goes back to the first stage. One rejected permanently, or ended in a hire, keeps its stage, and its new CV and cover note are kept without telling anyone. Otherwise the hiring team is told of a new CV, a new cover note or a reopened application, never of a retry. The tags are added to the candidate either way; external_ref stays the one sent first.

Scope: `write:candidate_applications`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [CandidateApplicationBody](#candidateapplicationbody)

| Answer | Body |
|---|---|
| `200` | [CandidateApplication](#candidateapplication) |
| `201` | [CandidateApplication](#candidateapplication) |
| `400` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/candidates

List candidates, oldest first.

Scope: `read:candidates`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no | How many to return, between 1 and 100. |
| `starting_after` | query | string, nullable | no | A candidate id: return the ones after it. |
| `email` | query | string, nullable | no | Only the candidate with this e-mail, exactly as stored. |
| `updated_after` | query | timestamp, nullable | no | Only candidates created or changed at or after this instant. |

| Answer | Body |
|---|---|
| `200` | [CandidateListResponse](#candidatelistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### GET /v1/candidates/{id}

Retrieve a candidate.

Scope: `read:candidates`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [Candidate](#candidate) |
| `400` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/cost_centers

List cost centers, oldest first.

Scope: `read:cost_centers`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no | How many to return, between 1 and 100. |
| `starting_after` | query | string, nullable | no | A cost center id: return the ones after it. |
| `updated_after` | query | timestamp, nullable | no | Return only cost centers created or changed at or after this instant. |

| Answer | Body |
|---|---|
| `200` | [CostCenterListResponse](#costcenterlistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### POST /v1/cost_centers

Create a cost center.

Scope: `write:cost_centers`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [CreateCostCenterRequest](#createcostcenterrequest)

| Answer | Body |
|---|---|
| `201` | [CostCenter](#costcenter) |
| `400` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### GET /v1/cost_centers/{id}

Retrieve a cost center.

Scope: `read:cost_centers`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [CostCenter](#costcenter) |
| `400` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### PATCH /v1/cost_centers/{id}

Update a cost center. A field left out keeps its value.

Scope: `write:cost_centers`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [UpdateCostCenterRequest](#updatecostcenterrequest)

| Answer | Body |
|---|---|
| `200` | [CostCenter](#costcenter) |
| `400` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/employees

List all employees

Scope: `read:employees`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no | A limit on the number of objects to be returned, between 1 and 100. |
| `starting_after` | query | string, nullable | no | A cursor for use in pagination. starting_after is an object ID that defines your place in the list. |
| `ending_before` | query | string, nullable | no | A cursor for use in pagination. ending_before is an object ID that defines your place in the list. |

| Answer | Body |
|---|---|
| `200` | [EmployeeListResponse](#employeelistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### GET /v1/employees/{id}

Retrieve a specific employee

Scope: `read:employees`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the employee to retrieve |

| Answer | Body |
|---|---|
| `200` | [Employee](#employee) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/job_openings

List job openings, oldest first. Open ones by default.

Scope: `read:job_openings`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `status` | query | string, nullable | no | open (the default), closed or archived. |
| `limit` | query | integer, nullable | no | How many to return, between 1 and 100. |
| `starting_after` | query | string, nullable | no | A job opening id: return the ones after it. |

| Answer | Body |
|---|---|
| `200` | [JobOpeningListResponse](#jobopeninglistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### POST /v1/job_openings

Create a job opening: open, not published, on the workspace's default pipeline, with no hiring team. The same external_ref again returns the job already created.

Scope: `write:job_openings`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [CreateJobOpeningBody](#createjobopeningbody)

| Answer | Body |
|---|---|
| `201` | [JobOpening](#jobopening) |
| `400` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### GET /v1/job_openings/{id}

Retrieve a job opening.

Scope: `read:job_openings`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [JobOpening](#jobopening) |
| `400` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/org_unit_types

List all org unit types

Scope: `read:org_unit_types`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no | A limit on the number of objects to be returned, between 1 and 100. |
| `starting_after` | query | string, nullable | no | A cursor for use in pagination. starting_after is an object ID that defines your place in the list. |
| `ending_before` | query | string, nullable | no | A cursor for use in pagination. ending_before is an object ID that defines your place in the list. |

| Answer | Body |
|---|---|
| `200` | [OrgUnitTypeListResponse](#orgunittypelistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### POST /v1/org_unit_types

Create a new org unit type

Scope: `write:org_unit_types`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [CreateOrgUnitTypeRequest](#createorgunittyperequest)

| Answer | Body |
|---|---|
| `201` | [OrgUnitType](#orgunittype) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### GET /v1/org_unit_types/{id}

Retrieve a specific org unit type

Scope: `read:org_unit_types`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the org unit type to retrieve |

| Answer | Body |
|---|---|
| `200` | [OrgUnitType](#orgunittype) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### PATCH /v1/org_unit_types/{id}

Update an existing org unit type

Scope: `write:org_unit_types`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the org unit type to update |
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [UpdateOrgUnitTypeRequest](#updateorgunittyperequest)

| Answer | Body |
|---|---|
| `200` | [OrgUnitType](#orgunittype) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### DELETE /v1/org_unit_types/{id}

Delete an org unit type

Scope: `write:org_unit_types`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the org unit type to delete |

| Answer | Body |
|---|---|
| `200` | [DeletedResponse](#deletedresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/org_units

List all org units

Scope: `read:org_units`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no | A limit on the number of objects to be returned, between 1 and 100. |
| `starting_after` | query | string, nullable | no | A cursor for use in pagination. starting_after is an object ID that defines your place in the list. |
| `ending_before` | query | string, nullable | no | A cursor for use in pagination. ending_before is an object ID that defines your place in the list. |

| Answer | Body |
|---|---|
| `200` | [OrgUnitListResponse](#orgunitlistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### POST /v1/org_units

Create a new org unit

Scope: `write:org_units`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [CreateOrgUnitRequest](#createorgunitrequest)

| Answer | Body |
|---|---|
| `201` | [OrgUnit](#orgunit) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### GET /v1/org_units/{id}

Retrieve a specific org unit

Scope: `read:org_units`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the org unit to retrieve |

| Answer | Body |
|---|---|
| `200` | [OrgUnit](#orgunit) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### PATCH /v1/org_units/{id}

Update an existing org unit

Scope: `write:org_units`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the org unit to update |
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [UpdateOrgUnitRequest](#updateorgunitrequest)

| Answer | Body |
|---|---|
| `200` | [OrgUnit](#orgunit) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### DELETE /v1/org_units/{id}

Delete an org unit

Scope: `write:org_units`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the org unit to delete |

| Answer | Body |
|---|---|
| `200` | [DeletedResponse](#deletedresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/webhook_endpoints

List all webhook endpoints

Scope: `read:webhook_endpoints`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no | A limit on the number of objects to be returned, between 1 and 100. |
| `starting_after` | query | string, nullable | no | A cursor for use in pagination. starting_after is an object ID that defines your place in the list. |
| `ending_before` | query | string, nullable | no | A cursor for use in pagination. ending_before is an object ID that defines your place in the list. |

| Answer | Body |
|---|---|
| `200` | [WebhookEndpointListResponse](#webhookendpointlistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### POST /v1/webhook_endpoints

Create a new webhook endpoint

Scope: `write:webhook_endpoints`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [CreateWebhookEndpointRequest](#createwebhookendpointrequest)

| Answer | Body |
|---|---|
| `201` | [WebhookEndpoint](#webhookendpoint) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### GET /v1/webhook_endpoints/event_types

List all available webhook event types

Scope: `read:webhook_endpoints`.

| Answer | Body |
|---|---|
| `200` | [WebhookEventTypes](#webhookeventtypes) |
| `401` | [ErrorResponse](#errorresponse) |

### GET /v1/webhook_endpoints/{id}

Retrieve a specific webhook endpoint

Scope: `read:webhook_endpoints`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the webhook endpoint to retrieve |

| Answer | Body |
|---|---|
| `200` | [WebhookEndpoint](#webhookendpoint) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### PATCH /v1/webhook_endpoints/{id}

Update an existing webhook endpoint

Scope: `write:webhook_endpoints`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the webhook endpoint to update |
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [UpdateWebhookEndpointRequest](#updatewebhookendpointrequest)

| Answer | Body |
|---|---|
| `200` | [WebhookEndpoint](#webhookendpoint) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### DELETE /v1/webhook_endpoints/{id}

Delete a webhook endpoint

Scope: `write:webhook_endpoints`.

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes | The ID of the webhook endpoint to delete |

| Answer | Body |
|---|---|
| `200` | [DeletedResponse](#deletedresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

## Types

### Candidate

A candidate, as the system that sent the application needs them back: who they are and the tags they carry. No applications, comments, CV or pipeline position; those stay with the hiring team.

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `candidate` | yes | String representing the object's type. Value is "candidate". |
| `id` | id (`cand_…`) | yes | Unique identifier, e.g. cand_01h2x…. |
| `first_name` | string, nullable | no | The candidate's first name, as given on their first application. |
| `last_name` | string, nullable | no | The candidate's last name, as given on their first application. |
| `email` | string, nullable | no | The e-mail the candidate is known by; one candidate per e-mail in a workspace. |
| `phone` | string, nullable | no | Phone number, when given. |
| `headline` | string, nullable | no | A one-line headline, when a recruiter wrote one. |
| `tags` | array of string, nullable | no | The candidate's tags by name, e.g. #frontend-engineer. |
| `created_at` | timestamp | no | Time at which the candidate was created. |
| `updated_at` | timestamp | no | Time at which the candidate or their tags last changed; the creation time until then. |

### CandidateApplication

An application a candidate made to one job opening.

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `candidate_application` | yes | String representing the object's type. Value is "candidate_application". |
| `id` | id (`capp_…`) | yes | Unique identifier, e.g. capp_01h2x…. |
| `job_opening_id` | id (`job_…`) | no | The job opening applied to. |
| `candidate_id` | id (`cand_…`) | no | The candidate, reused when the e-mail is already known. |
| `external_ref` | string, nullable | no | Your id for the submission, as sent. |
| `has_cv` | boolean | no | Whether a CV was stored with the application. |
| `cv_version` | integer, nullable | no | The number of the current CV among the CVs the application has had, starting at 1; absent with no CV. A candidate who applies again with a new CV moves it up by one, and the earlier CVs stay with the application. |
| `updated` | boolean | no | On a 200, whether this request changed the application that already existed: a new CV, a new cover note, or a closed application reopened. false on a 201, on a retry, and on a request whose only news is a phone number, which goes onto the candidate rather than the application. |
| `created_at` | timestamp | no | Time at which the application was made. |

### CandidateApplicationBody

The body of POST /v1/candidate_applications, as JSON or as the fields of a multipart/form-data request that also carries the CV in a file field named cv. In a form, tags repeats or is comma-separated.

| Field | Type | Required | Description |
|---|---|---|---|
| `job_opening_id` | string, nullable | no | Required: an open job opening, job_…. |
| `first_name` | string, nullable | no | Required. Used when the candidate is new; an existing candidate, found by email, keeps the name Thrivea holds. |
| `last_name` | string, nullable | no | Required. Used when the candidate is new; an existing candidate keeps the name Thrivea holds. |
| `email` | string, nullable | no | Required. The candidate with this e-mail is reused. A second application to the same job is the first one, answered with 200: a new CV becomes its current CV (the old one is kept), a new cover note and phone replace the old, and an application that was rejected, though not permanently, goes back to the first stage and the hiring team hears of it again. The same request sent twice changes nothing the second time. |
| `phone` | string, nullable | no | Optional. A number given here replaces the one the candidate had; an empty one keeps it. |
| `cover_note` | string, nullable | no | What the candidate wrote, and any form answers with no field of their own. |
| `tags` | array of string, nullable | no | Tag names, e.g. #frontend-engineer: found by name, created when missing, added to the candidate's own. |
| `external_ref` | string, nullable | no | Your id for the submission. Stored on the application. |

### CandidateListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `list` | yes | String representing the object's type. Value is "list". |
| `url` | string, nullable | no | URL of this list resource. |
| `data` | array of [Candidate](#candidate), nullable | no | The candidates on this page. |
| `has_more` | boolean | no | True if there are more candidates after this page. |

### CostCenter

A cost center: a code that job positions are charged to.

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `cost_center` | yes | String representing the object's type. Value is "cost_center". |
| `id` | id (`cc_…`) | yes | Unique identifier for the cost center, e.g. cc_01h2xcejqtf2nbrexx3vqjhp41. |
| `code` | string, nullable | no | The cost center's code, at most 20 characters. |
| `job_position_ids` | array of id (`pos_…`), nullable | no | The job positions charged to this cost center. |
| `active` | boolean | no | Whether the cost center is in use. |
| `created_at` | timestamp | no | Time at which the cost center was created. |
| `updated_at` | timestamp | no | Time at which the cost center last changed; equal to created_at until it does. |

### CostCenterListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `list` | yes | String representing the object's type. Value is "list". |
| `url` | string, nullable | no | URL of this list resource. |
| `data` | array of [CostCenter](#costcenter), nullable | no | The cost centers on this page. |
| `has_more` | boolean | no | True if there are more cost centers after this page. |

### CreateCostCenterRequest

The body of POST /v1/cost_centers.

| Field | Type | Required | Description |
|---|---|---|---|
| `code` | string, nullable | no | The cost center's code, at most 20 characters. |
| `job_position_ids` | array of id (`pos_…`), nullable | no | The job positions charged to it. |

### CreateJobOpeningBody

The body of POST /v1/job_openings.

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | string, nullable | no | Required. |
| `description` | string, nullable | no | Plain text or basic HTML; may be the ad's summary and a link to its page. |
| `workplace_type` | string, nullable | no | Required: onsite, remote or hybrid. |
| `external_ref` | string, nullable | no | Your id for the job. Sending the same one again returns the job already created, never a second one. |

### CreateOrgUnitRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string, nullable | no | Name of the org unit |
| `description` | string, nullable | no | Description of the org unit |
| `org_unit_type_ids` | array of id (`orgut_…`), nullable | no | IDs of the org unit types this org unit belongs to |
| `parent_org_unit_ids` | array of id (`orgu_…`), nullable | no | IDs of the parent org units |
| `sibling_org_unit_ids` | array of id (`orgu_…`), nullable | no | IDs of the sibling org units |
| `manager_ids` | array of id (`emp_…`), nullable | no | Refused: who manages an org unit comes from each person's work history. Sending it is a 400 naming the field. |
| `employee_ids` | array of id (`emp_…`), nullable | no | Refused: who belongs to an org unit comes from each person's work history. Sending it is a 400 naming the field. |
| `managers_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no | Refused: who manages an org unit comes from each person's work history. Sending it is a 400 naming the field. |
| `employees_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no | Refused: who belongs to an org unit comes from each person's work history. Sending it is a 400 naming the field. |
| `public_id` | string, nullable | no | Public id of the org unit |
| `active_from` | timestamp, nullable | no | Date when the org unit becomes active (optional, UTC). If not specified or in the past, the org unit is immediately active. |
| `active_to` | timestamp, nullable | no | Date when the org unit should be archived (optional, UTC). If not specified, the org unit never auto-archives. |
| `external_code` | string, nullable | no | External code of the org unit |

### CreateOrgUnitTypeRequest

The body of POST /v1/org_unit_types.

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string, nullable | no | Required. The type's name. |
| `description` | string, nullable | no | Optional. What the type is for. |
| `hex_color` | string, nullable | no | Required. A hex colour code such as #4F46E5. |

### CreateWebhookEndpointRequest

The body of POST /v1/webhook_endpoints. The answer carries the signing secret once, in the Thrivea-Webhook-Secret header.

| Field | Type | Required | Description |
|---|---|---|---|
| `url` | string, nullable | no | Required. An https address whose host resolves to public addresses only; a private, loopback, link-local or metadata address is a 400. |
| `description` | string, nullable | no | What the endpoint is for, for your own records. |
| `event_types` | array of string, nullable | no | Required, one or more types from GET /v1/webhook_endpoints/event_types. An unknown type is a 400; an employee.* type asked for by an integration key is a 403. |
| `enabled` | boolean | no | Whether to send events to it. Read as true when left out. |

### DeletedResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | string, nullable | no |  |
| `object` | string, nullable | no |  |
| `deleted` | boolean | no |  |

### Employee

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | id (`emp_…`) | no | Unique identifier for the employee, e.g. emp_01h2xcejqtf2nbrexx3vqjhp41. |
| `object` | string, always `employee` | yes | String representing the object's type. Value is "employee". |
| `first_name` | string, nullable | no | First name of the employee |
| `middle_name` | string, nullable | no | Middle name of the employee, if any |
| `last_name` | string, nullable | no | Last name of the employee |
| `display_name` | string, nullable | no | Display name of the employee |
| `email_address` | string, nullable | no | Email address of the employee |
| `profile_picture_url` | string, nullable | no | URL of the employee's profile picture, if any |
| `birth_date` | timestamp, nullable | no | Birthdate of the employee, if available |
| `profile_cover_picture_url` | string, nullable | no | URL of the employee's profile cover picture, if any |
| `status` | [EmployeeStatus](#employeestatus) | no | Current status of the employee |

### EmployeeListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `list` | yes | String representing the object's type. Value is "list". |
| `url` | string, nullable | no | URL of this list resource |
| `data` | array of [Employee](#employee), nullable | no | List of employees |
| `has_more` | boolean | no | True if there are more items available on this page |

### EmployeeStatus

Represents the current status of an employee in the system

| Value | Meaning |
|---|---|
| `1` | Invited |
| `2` | Active |
| `3` | Deactivated |
| `4` | PendingDeactivation |

### ErrorDetails

| Field | Type | Required | Description |
|---|---|---|---|
| `type` | string, nullable | no |  |
| `message` | string, nullable | no |  |
| `param` | string, nullable | no |  |
| `code` | string, nullable | no |  |
| `doc_url` | string, nullable | no |  |
| `request_id` | string, nullable | no |  |

### ErrorResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `error` | [ErrorDetails](#errordetails), nullable | no |  |

### JobOpening

A job opening, as another system needs it to attach applications: no hiring team, no candidate counts, no pipeline.

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `job_opening` | yes | String representing the object's type. Value is "job_opening". |
| `id` | id (`job_…`) | yes | Unique identifier, e.g. job_01h2xcejqtf2nbrexx3vqjhp41. |
| `title` | string, nullable | no | The job's title. |
| `status` | string, nullable | no | open, closed or archived. Applications are accepted only while it is open. |
| `workplace_type` | string, nullable | no | onsite, remote or hybrid. |
| `published` | boolean | no | Whether the job is on the careers page. A job created through the API is not, until a recruiter publishes it. |
| `external_ref` | string, nullable | no | The id your system gave the job when it created it, if it did. |
| `created_at` | timestamp | no | Time at which the job was created. |

### JobOpeningListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `list` | yes | String representing the object's type. Value is "list". |
| `url` | string, nullable | no | URL of this list resource. |
| `data` | array of [JobOpening](#jobopening), nullable | no | The job openings on this page. |
| `has_more` | boolean | no | True if there are more job openings after this page. |

### OrgUnit

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | id (`orgu_…`) | yes | Unique identifier for the org unit, e.g. orgu_01h2xcejqtf2nbrexx3vqjhp41. |
| `object` | string, always `org_unit` | yes | String representing the object's type. Value is "org_unit". |
| `name` | string, nullable | no | Name of the org unit |
| `description` | string, nullable | no | Description of the org unit |
| `org_unit_type_ids` | array of id (`orgut_…`), nullable | no | IDs of the org unit types this org unit belongs to |
| `parent_org_unit_ids` | array of id (`orgu_…`), nullable | no | IDs of the parent org units |
| `sibling_org_unit_ids` | array of id (`orgu_…`), nullable | no | IDs of the sibling org units |
| `manager_ids` | array of id (`emp_…`), nullable | no | Who manages this org unit, emp_ ids. Read-only: it comes from each person's work history. |
| `employee_ids` | array of id (`emp_…`), nullable | no | Who belongs to this org unit, emp_ ids. Read-only: it comes from each person's work history. |
| `managers_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no | The rule behind manager_ids, kept for compatibility: by_employee_ids rules derived from work history. |
| `employees_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource) | yes | The rule behind employee_ids, kept for compatibility: by_employee_ids rules derived from work history. |
| `status` | [OrgUnitStatus](#orgunitstatus) | no | Current status of the org unit |
| `active_from` | timestamp, nullable | no | Date when the org unit becomes active (optional, UTC) |
| `active_to` | timestamp, nullable | no | Date when the org unit should be archived (optional, UTC) |
| `created_at` | timestamp | no | Time at which the org unit was created |
| `created_by` | id (`emp_…`) | no | ID of the user who created the org unit |
| `updated_at` | timestamp | no | Time at which the org unit was last updated |
| `updated_by` | id (`emp_…`) | no | ID of the user who last updated the org unit |
| `external_code` | string, nullable | no | Non-unique external code for the org unit (e.g. a department or cost-centre code). Unlike public_id, this value does not need to be unique across org units. |

### OrgUnitListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `list` | yes | String representing the object's type. Value is "list". |
| `url` | string, nullable | no | URL of this list resource |
| `data` | array of [OrgUnit](#orgunit), nullable | no | List of org units |
| `has_more` | boolean | no | True if there are more items available on this page |

### OrgUnitStatus

Represents the current status of an organizational unit in the system

| Value | Meaning |
|---|---|
| `1` | Active |
| `2` | Archived |
| `3` | Pending |
| `4` | Deleted |

### OrgUnitType

A kind of org unit, such as Department or Team, with the colour the org chart draws it in.

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | id (`orgut_…`) | no | Unique identifier, e.g. orgut_01h2x…. |
| `object` | string, always `org_unit_type` | yes | String representing the object's type. Value is "org_unit_type". |
| `name` | string, nullable | no | The type's name. |
| `description` | string, nullable | no | What the type is for; empty when none was given. |
| `hex_color` | string | yes | The colour the org chart uses for units of this type, as a hex code such as #4F46E5. |
| `status` | [OrgUnitTypeStatus](#orgunittypestatus) | no |  |
| `created_at` | timestamp | no | Time at which the type was created. |
| `updated_at` | timestamp | no | Time of the last change; the creation time until the first one. |

### OrgUnitTypeListResponse

A page of org unit types.

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `list` | yes | String representing the object's type. Value is "list". |
| `url` | string, nullable | no | URL of this list resource. |
| `has_more` | boolean | no | True if there are more types after this page. |
| `data` | array of [OrgUnitType](#orgunittype), nullable | no | The types on this page. |

### OrgUnitTypeStatus

Represents the current status of an organizational unit type in the system

| Value | Meaning |
|---|---|
| `1` | Active |
| `2` | Archived |

### PeopleCriteriaResource

Who manages, or who belongs to, an org unit: a rule, not a list. The people matched by any rule in Includes, minus the people matched by any rule in Excludes.

| Field | Type | Required | Description |
|---|---|---|---|
| `includes` | array of [PeopleCriterionResource](#peoplecriterionresource), nullable | no | The rules a person must match one of to be in the group. |
| `excludes` | array of [PeopleCriterionResource](#peoplecriterionresource), nullable | no | The rules that take a person out of the group even when an include matches. |

### PeopleCriterionResource

One rule. Type names it (the snake_case tokens in OrgUnitCriteria) and the one field that rule takes carries its argument; the fields the rule does not take are left out of the JSON. Ids are the prefixed ids the rest of /v1 uses where a prefix is registered for the kind.

| Field | Type | Required | Description |
|---|---|---|---|
| `type` | string, nullable | no | The rule: one of everyone, none, by_employee_ids, is_in_org_unit, is_org_unit_manager, has_job_position, works_at_same_site, is_in_permission_group, has_permission, has_erb_scalar_field, reports_to, is_direct_manager, is_indirect_manager, is_self, is_buddy, is_hrbp or is_it_admin. The field the rule needs is set beside it; treat a type you do not know as one added later. |
| `employee_ids` | array of id (`emp_…`), nullable | no | by_employee_ids: the people named. |
| `org_unit_id` | id (`orgu_…`), nullable | no | is_in_org_unit: the members of that unit. |
| `job_position_id` | id (`pos_…`), nullable | no | has_job_position: the people in that job position. |
| `site_id` | id (`site_…`), nullable | no | works_at_same_site: the people at that site. |
| `permission_group_id` | uuid, nullable | no | is_in_permission_group: the members of that permission group. No prefix is registered for groups, so a plain GUID. |
| `permission_name` | string, nullable | no | has_permission: the people who hold that permission, by its stored name. |
| `erb_field_id` | uuid, nullable | no | has_erb_scalar_field: the people with a value in that employee record field. No prefix is registered for fields, so a plain GUID. |

### UpdateCostCenterRequest

The body of PATCH /v1/cost_centers/{id}. A field left out keeps its value.

| Field | Type | Required | Description |
|---|---|---|---|
| `code` | string, nullable | no | The cost center's new code. |
| `job_position_ids` | array of id (`pos_…`), nullable | no | The whole list of job positions charged to it; it replaces the list it had. |

### UpdateOrgUnitRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string | yes | Name of the org unit |
| `description` | string, nullable | no | Description of the org unit |
| `org_unit_type_ids` | array of id (`orgut_…`) | yes | IDs of the org unit types this org unit belongs to |
| `parent_org_unit_ids` | array of id (`orgu_…`), nullable | no | IDs of the parent org units |
| `sibling_org_unit_ids` | array of id (`orgu_…`), nullable | no | IDs of the sibling org units |
| `manager_ids` | array of id (`emp_…`), nullable | no | Refused: who manages an org unit comes from each person's work history. Sending it is a 400 naming the field. |
| `employee_ids` | array of id (`emp_…`) | yes | Refused: who belongs to an org unit comes from each person's work history. Sending it is a 400 naming the field. |
| `managers_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no | Refused: who manages an org unit comes from each person's work history. Sending it is a 400 naming the field. |
| `employees_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no | Refused: who belongs to an org unit comes from each person's work history. Sending it is a 400 naming the field. |
| `status` | [OrgUnitStatus](#orgunitstatus) | no | Status of the organizational unit, indicating whether it is active or archived. |
| `public_id` | string, nullable | no | Public id of the org unit |
| `active_from` | timestamp, nullable | no | Date when the org unit becomes active (optional, UTC). If not specified or in the past, the org unit is immediately active. |
| `active_to` | timestamp, nullable | no | Date when the org unit should be archived (optional, UTC). If not specified, the org unit never auto-archives. |
| `external_code` | string, nullable | no | External code of the org unit, used as an optional external or business identifier. |

### UpdateOrgUnitTypeRequest

The body of PATCH /v1/org_unit_types/{id}. A field left out keeps its current value.

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string, nullable | no | The new name; cannot be empty. |
| `description` | string, nullable | no | The new description. |
| `hex_color` | string, nullable | no | The new hex colour code, such as #4F46E5. |
| `status` | [OrgUnitTypeStatus](#orgunittypestatus) | no | 1 (active) or 2 (archived); left out, the status stays. |

### UpdateWebhookEndpointRequest

The body of PATCH /v1/webhook_endpoints/{id}. It replaces the endpoint's URL, description, event types and enabled, so send all four.

| Field | Type | Required | Description |
|---|---|---|---|
| `url` | string, nullable | no | Required. An https address whose host resolves to public addresses only; a private, loopback, link-local or metadata address is a 400. |
| `description` | string, nullable | no | What the endpoint is for, for your own records. |
| `event_types` | array of string, nullable | no | Required, one or more types from GET /v1/webhook_endpoints/event_types. An unknown type is a 400; an employee.* type asked for by an integration key is a 403. |
| `enabled` | boolean | no | Whether to send events to it. Read as true when left out. |

### WebhookEndpoint

An HTTPS address Thrivea posts events to, and which events it receives.

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | id (`we_…`) | no | Unique identifier, e.g. we_01h2x…. |
| `object` | string, always `webhook_endpoint` | yes | String representing the object's type. Value is "webhook_endpoint". |
| `url` | string, nullable | no | The https address events are posted to. Its host must resolve to public addresses only, checked again before every delivery. |
| `description` | string, nullable | no | What the endpoint is for, for your own records; may be null. |
| `enabled` | boolean | no | Whether events are sent to it. Thrivea sets it to false when the endpoint answers 410 Gone. |
| `event_types` | array of string, nullable | no | The event types it receives, from GET /v1/webhook_endpoints/event_types. |
| `created_at` | timestamp | no | Time at which the endpoint was created. |
| `updated_at` | timestamp, nullable | no | Time of the last change; null until the first one. |

### WebhookEndpointListResponse

A page of webhook endpoints.

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, always `list` | yes | String representing the object's type. Value is "list". |
| `url` | string, nullable | no | URL of this list resource. |
| `has_more` | boolean | no | True if there are more endpoints after this page. |
| `data` | array of [WebhookEndpoint](#webhookendpoint), nullable | no | The endpoints on this page. |

### WebhookEventTypes

Type: object
