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
| [`POST /v1/candidate_applications`](#post-v1candidate_applications) |  | see the resource page |
| [`GET /v1/candidates`](#get-v1candidates) |  | see the resource page |
| [`GET /v1/candidates/{id}`](#get-v1candidatesid) |  | see the resource page |
| [`GET /v1/cost_centers`](#get-v1cost_centers) |  | see the resource page |
| [`POST /v1/cost_centers`](#post-v1cost_centers) |  | see the resource page |
| [`GET /v1/cost_centers/{id}`](#get-v1cost_centersid) |  | see the resource page |
| [`PATCH /v1/cost_centers/{id}`](#patch-v1cost_centersid) |  | see the resource page |
| [`GET /v1/employees`](#get-v1employees) |  | see the resource page |
| [`GET /v1/employees/{id}`](#get-v1employeesid) |  | see the resource page |
| [`GET /v1/job_openings`](#get-v1job_openings) |  | see the resource page |
| [`POST /v1/job_openings`](#post-v1job_openings) |  | see the resource page |
| [`GET /v1/job_openings/{id}`](#get-v1job_openingsid) |  | see the resource page |
| [`GET /v1/org_unit_types`](#get-v1org_unit_types) |  | see the resource page |
| [`POST /v1/org_unit_types`](#post-v1org_unit_types) |  | see the resource page |
| [`GET /v1/org_unit_types/{id}`](#get-v1org_unit_typesid) |  | see the resource page |
| [`PATCH /v1/org_unit_types/{id}`](#patch-v1org_unit_typesid) |  | see the resource page |
| [`DELETE /v1/org_unit_types/{id}`](#delete-v1org_unit_typesid) |  | see the resource page |
| [`GET /v1/org_units`](#get-v1org_units) |  | see the resource page |
| [`POST /v1/org_units`](#post-v1org_units) |  | see the resource page |
| [`GET /v1/org_units/{id}`](#get-v1org_unitsid) |  | see the resource page |
| [`PATCH /v1/org_units/{id}`](#patch-v1org_unitsid) |  | see the resource page |
| [`DELETE /v1/org_units/{id}`](#delete-v1org_unitsid) |  | see the resource page |
| [`GET /v1/webhook_endpoints`](#get-v1webhook_endpoints) |  | see the resource page |
| [`POST /v1/webhook_endpoints`](#post-v1webhook_endpoints) |  | see the resource page |
| [`GET /v1/webhook_endpoints/event_types`](#get-v1webhook_endpointsevent_types) |  | see the resource page |
| [`GET /v1/webhook_endpoints/{id}`](#get-v1webhook_endpointsid) |  | see the resource page |
| [`PATCH /v1/webhook_endpoints/{id}`](#patch-v1webhook_endpointsid) |  | see the resource page |
| [`DELETE /v1/webhook_endpoints/{id}`](#delete-v1webhook_endpointsid) |  | see the resource page |

### POST /v1/candidate_applications

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

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no |  |
| `starting_after` | query | string, nullable | no |  |
| `email` | query | string, nullable | no |  |
| `updated_after` | query | timestamp, nullable | no |  |

| Answer | Body |
|---|---|
| `200` | [CandidateListResponse](#candidatelistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### GET /v1/candidates/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [Candidate](#candidate) |
| `400` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/cost_centers

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no |  |
| `starting_after` | query | string, nullable | no |  |
| `updated_after` | query | timestamp, nullable | no |  |

| Answer | Body |
|---|---|
| `200` | [CostCenterListResponse](#costcenterlistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### POST /v1/cost_centers

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

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [CostCenter](#costcenter) |
| `400` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### PATCH /v1/cost_centers/{id}

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

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no |  |
| `starting_after` | query | string, nullable | no |  |
| `ending_before` | query | string, nullable | no |  |

| Answer | Body |
|---|---|
| `200` | [EmployeeListResponse](#employeelistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### GET /v1/employees/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [Employee](#employee) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/job_openings

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `status` | query | string, nullable | no |  |
| `limit` | query | integer, nullable | no |  |
| `starting_after` | query | string, nullable | no |  |

| Answer | Body |
|---|---|
| `200` | [JobOpeningListResponse](#jobopeninglistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |

### POST /v1/job_openings

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

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [JobOpening](#jobopening) |
| `400` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/org_unit_types

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no |  |
| `starting_after` | query | string, nullable | no |  |
| `ending_before` | query | string, nullable | no |  |

| Answer | Body |
|---|---|
| `200` | [OrgUnitTypeListResponse](#orgunittypelistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### POST /v1/org_unit_types

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

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [OrgUnitType](#orgunittype) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### PATCH /v1/org_unit_types/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [UpdateOrgUnitTypeRequest](#updateorgunittyperequest)

| Answer | Body |
|---|---|
| `200` | [OrgUnitType](#orgunittype) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### DELETE /v1/org_unit_types/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [DeletedResponse](#deletedresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/org_units

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no |  |
| `starting_after` | query | string, nullable | no |  |
| `ending_before` | query | string, nullable | no |  |

| Answer | Body |
|---|---|
| `200` | [OrgUnitListResponse](#orgunitlistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### POST /v1/org_units

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

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [OrgUnit](#orgunit) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### PATCH /v1/org_units/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [UpdateOrgUnitRequest](#updateorgunitrequest)

| Answer | Body |
|---|---|
| `200` | [OrgUnit](#orgunit) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### DELETE /v1/org_units/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [DeletedResponse](#deletedresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### GET /v1/webhook_endpoints

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `limit` | query | integer, nullable | no |  |
| `starting_after` | query | string, nullable | no |  |
| `ending_before` | query | string, nullable | no |  |

| Answer | Body |
|---|---|
| `200` | [WebhookEndpointListResponse](#webhookendpointlistresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |

### POST /v1/webhook_endpoints

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

| Answer | Body |
|---|---|
| `200` | [WebhookEventTypes](#webhookeventtypes) |
| `401` | [ErrorResponse](#errorresponse) |

### GET /v1/webhook_endpoints/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [WebhookEndpoint](#webhookendpoint) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `403` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### PATCH /v1/webhook_endpoints/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |
| `Idempotency-Key` | header | string | no | A key of your choosing (1 to 255 characters) that makes this write safe to retry: the first 2xx answer is kept for 24 hours and replayed to a request with the same key, with Idempotent-Replayed: true. The same key with a different request is a 400; the same key while the first request is still running is a 409. |

Body (`application/json`): [UpdateWebhookEndpointRequest](#updatewebhookendpointrequest)

| Answer | Body |
|---|---|
| `200` | [WebhookEndpoint](#webhookendpoint) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

### DELETE /v1/webhook_endpoints/{id}

| Parameter | In | Type | Required | Description |
|---|---|---|---|---|
| `id` | path | string | yes |  |

| Answer | Body |
|---|---|
| `200` | [DeletedResponse](#deletedresponse) |
| `400` | [ErrorResponse](#errorresponse) |
| `401` | [ErrorResponse](#errorresponse) |
| `404` | [ErrorResponse](#errorresponse) |

## Types

### Candidate

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `id` | uuid | yes |  |
| `first_name` | string, nullable | no |  |
| `last_name` | string, nullable | no |  |
| `email` | string, nullable | no |  |
| `phone` | string, nullable | no |  |
| `headline` | string, nullable | no |  |
| `tags` | array of string, nullable | no |  |
| `created_at` | timestamp | no |  |
| `updated_at` | timestamp | no |  |

### CandidateApplication

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `id` | uuid | yes |  |
| `job_opening_id` | uuid | no |  |
| `candidate_id` | uuid | no |  |
| `external_ref` | string, nullable | no |  |
| `has_cv` | boolean | no |  |
| `cv_version` | integer, nullable | no |  |
| `updated` | boolean | no |  |
| `created_at` | timestamp | no |  |

### CandidateApplicationBody

| Field | Type | Required | Description |
|---|---|---|---|
| `job_opening_id` | string, nullable | no |  |
| `first_name` | string, nullable | no |  |
| `last_name` | string, nullable | no |  |
| `email` | string, nullable | no |  |
| `phone` | string, nullable | no |  |
| `cover_note` | string, nullable | no |  |
| `tags` | array of string, nullable | no |  |
| `external_ref` | string, nullable | no |  |

### CandidateListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `url` | string, nullable | no |  |
| `data` | array of [Candidate](#candidate), nullable | no |  |
| `has_more` | boolean | no |  |

### CostCenter

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `id` | uuid | yes |  |
| `code` | string, nullable | no |  |
| `job_position_ids` | array of uuid, nullable | no |  |
| `active` | boolean | no |  |
| `created_at` | timestamp | no |  |
| `updated_at` | timestamp | no |  |

### CostCenterListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `url` | string, nullable | no |  |
| `data` | array of [CostCenter](#costcenter), nullable | no |  |
| `has_more` | boolean | no |  |

### CreateCostCenterRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `code` | string, nullable | no |  |
| `job_position_ids` | array of uuid, nullable | no |  |

### CreateJobOpeningBody

| Field | Type | Required | Description |
|---|---|---|---|
| `title` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `workplace_type` | string, nullable | no |  |
| `external_ref` | string, nullable | no |  |

### CreateOrgUnitRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `org_unit_type_ids` | array of uuid, nullable | no |  |
| `parent_org_unit_ids` | array of uuid, nullable | no |  |
| `sibling_org_unit_ids` | array of uuid, nullable | no |  |
| `manager_ids` | array of uuid, nullable | no |  |
| `employee_ids` | array of uuid, nullable | no |  |
| `managers_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no |  |
| `employees_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no |  |
| `public_id` | string, nullable | no |  |
| `active_from` | timestamp, nullable | no |  |
| `active_to` | timestamp, nullable | no |  |
| `external_code` | string, nullable | no |  |

### CreateOrgUnitTypeRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `hex_color` | string, nullable | no |  |

### CreateWebhookEndpointRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `url` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `event_types` | array of string, nullable | no |  |
| `enabled` | boolean | no |  |

### DeletedResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | string, nullable | no |  |
| `object` | string, nullable | no |  |
| `deleted` | boolean | no |  |

### Employee

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | uuid | no |  |
| `object` | string, nullable | no |  |
| `first_name` | string, nullable | no |  |
| `middle_name` | string, nullable | no |  |
| `last_name` | string, nullable | no |  |
| `display_name` | string, nullable | no |  |
| `email_address` | string, nullable | no |  |
| `profile_picture_url` | string, nullable | no |  |
| `birth_date` | timestamp, nullable | no |  |
| `profile_cover_picture_url` | string, nullable | no |  |
| `status` | [EmployeeStatus](#employeestatus) | no |  |

### EmployeeListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `url` | string, nullable | no |  |
| `data` | array of [Employee](#employee), nullable | no |  |
| `has_more` | boolean | no |  |

### EmployeeStatus

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

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `id` | uuid | yes |  |
| `title` | string, nullable | no |  |
| `status` | string, nullable | no |  |
| `workplace_type` | string, nullable | no |  |
| `published` | boolean | no |  |
| `external_ref` | string, nullable | no |  |
| `created_at` | timestamp | no |  |

### JobOpeningListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `url` | string, nullable | no |  |
| `data` | array of [JobOpening](#jobopening), nullable | no |  |
| `has_more` | boolean | no |  |

### OrgUnit

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | uuid | yes |  |
| `object` | string, nullable | no |  |
| `name` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `org_unit_type_ids` | array of uuid, nullable | no |  |
| `parent_org_unit_ids` | array of uuid, nullable | no |  |
| `sibling_org_unit_ids` | array of uuid, nullable | no |  |
| `manager_ids` | array of uuid, nullable | no |  |
| `employee_ids` | array of uuid, nullable | no |  |
| `managers_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no |  |
| `employees_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource) | yes |  |
| `status` | [OrgUnitStatus](#orgunitstatus) | no |  |
| `active_from` | timestamp, nullable | no |  |
| `active_to` | timestamp, nullable | no |  |
| `created_at` | timestamp | no |  |
| `created_by` | uuid | no |  |
| `updated_at` | timestamp | no |  |
| `updated_by` | uuid | no |  |
| `external_code` | string, nullable | no |  |

### OrgUnitListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `url` | string, nullable | no |  |
| `data` | array of [OrgUnit](#orgunit), nullable | no |  |
| `has_more` | boolean | no |  |

### OrgUnitStatus

| Value | Meaning |
|---|---|
| `1` | Active |
| `2` | Archived |
| `3` | Pending |
| `4` | Deleted |

### OrgUnitType

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | uuid | no |  |
| `object` | string, nullable | no |  |
| `name` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `hex_color` | string | yes |  |
| `status` | [OrgUnitTypeStatus](#orgunittypestatus) | no |  |
| `created_at` | timestamp | no |  |
| `updated_at` | timestamp | no |  |

### OrgUnitTypeListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `url` | string, nullable | no |  |
| `has_more` | boolean | no |  |
| `data` | array of [OrgUnitType](#orgunittype), nullable | no |  |

### OrgUnitTypeStatus

| Value | Meaning |
|---|---|
| `1` | Active |
| `2` | Archived |

### PeopleCriteriaResource

| Field | Type | Required | Description |
|---|---|---|---|
| `includes` | array of [PeopleCriterionResource](#peoplecriterionresource), nullable | no |  |
| `excludes` | array of [PeopleCriterionResource](#peoplecriterionresource), nullable | no |  |

### PeopleCriterionResource

| Field | Type | Required | Description |
|---|---|---|---|
| `type` | string, nullable | no |  |
| `employee_ids` | array of uuid, nullable | no |  |
| `org_unit_id` | uuid, nullable | no |  |
| `job_position_id` | uuid, nullable | no |  |
| `site_id` | uuid, nullable | no |  |
| `permission_group_id` | uuid, nullable | no |  |
| `permission_name` | string, nullable | no |  |
| `erb_field_id` | uuid, nullable | no |  |

### UpdateCostCenterRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `code` | string, nullable | no |  |
| `job_position_ids` | array of uuid, nullable | no |  |

### UpdateOrgUnitRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string | yes |  |
| `description` | string, nullable | no |  |
| `org_unit_type_ids` | array of uuid | yes |  |
| `parent_org_unit_ids` | array of uuid, nullable | no |  |
| `sibling_org_unit_ids` | array of uuid, nullable | no |  |
| `manager_ids` | array of uuid, nullable | no |  |
| `employee_ids` | array of uuid | yes |  |
| `managers_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no |  |
| `employees_criteria` | [PeopleCriteriaResource](#peoplecriteriaresource), nullable | no |  |
| `status` | [OrgUnitStatus](#orgunitstatus) | no |  |
| `public_id` | string, nullable | no |  |
| `active_from` | timestamp, nullable | no |  |
| `active_to` | timestamp, nullable | no |  |
| `external_code` | string, nullable | no |  |

### UpdateOrgUnitTypeRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `name` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `hex_color` | string, nullable | no |  |
| `status` | [OrgUnitTypeStatus](#orgunittypestatus) | no |  |

### UpdateWebhookEndpointRequest

| Field | Type | Required | Description |
|---|---|---|---|
| `url` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `event_types` | array of string, nullable | no |  |
| `enabled` | boolean | no |  |

### WebhookEndpoint

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | uuid | no |  |
| `object` | string, nullable | no |  |
| `url` | string, nullable | no |  |
| `description` | string, nullable | no |  |
| `enabled` | boolean | no |  |
| `event_types` | array of string, nullable | no |  |
| `created_at` | timestamp | no |  |
| `updated_at` | timestamp, nullable | no |  |

### WebhookEndpointListResponse

| Field | Type | Required | Description |
|---|---|---|---|
| `object` | string, nullable | no |  |
| `url` | string, nullable | no |  |
| `has_more` | boolean | no |  |
| `data` | array of [WebhookEndpoint](#webhookendpoint), nullable | no |  |

### WebhookEventTypes

Type: object
