# Changelog

All notable changes to `Thrivea.Sdk` are recorded here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the package follows [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.4.0] - 2026-11-10

### Added

- `CandidateApplication.Updated` and `CandidateApplication.CvVersion`. A second application from the same candidate to the same job updates the first one: a new CV becomes the current version, a new cover note or phone replaces the old one, and a rejected or declined application goes back to the first stage. `Updated` says whether the request changed anything, and `CvVersion` is the number of the current CV.

## [0.3.0] - 2026-11-10

### Added

- `WebhookEventTypes.JobOpeningCreated`, `JobOpeningUpdated`, `JobOpeningPublished`, `JobOpeningUnpublished`, `JobOpeningClosed`, `JobOpeningReopened` and `JobOpeningArchived`. Their data is the `JobOpening` as the API returns it, so `GetData<JobOpening>()` reads it.

## [0.2.0] - 2026-11-08

### Added

- `OrgUnit.ManagerIds` and `OrgUnit.EmployeeIds`: who manages and who belongs to the unit, read-only, from each person's work history.
- `EmployeeCreatedData`, the data of an `employee.created` event.
- The SDK is tested against the API's OpenAPI document, vendored in the repository, with a nightly check that the copies agree.

### Changed

- `PeopleCriterion` has typed fields and the API's snake_case rule types.

### Deprecated

- The membership fields of the org unit create and update options. The API refuses them with `400`; membership comes from work history.

## [0.1.0] - 2026-11-07

### Added

- `ThriveaClient` with Bearer API-key authentication, per-request options, a configurable host and timeout, and retries with exponential backoff that honour `Retry-After`.
- Services for every `/v1` resource: `JobOpenings`, `CandidateApplications` (JSON or multipart with a CV), `Candidates`, `Employees`, `OrgUnits`, `OrgUnitTypes`, `CostCenters`, `WebhookEndpoints`.
- Cursor paging with `ThriveaList<T>` and `ListAutoPagingAsync`.
- `TypeId`: parse, create and check the prefixed identifiers the API uses, with the official specification's test vectors.
- Typed exceptions per error type: `ThriveaInvalidRequestException`, `ThriveaAuthenticationException`, `ThriveaPermissionException`, `ThriveaNotFoundException`, `ThriveaConflictException`, `ThriveaRateLimitException`, `ThriveaApiException`, `ThriveaNetworkException`.
- `WebhookSignature` and `WebhookEvent` for verifying and reading webhook deliveries.
- Targets `netstandard2.0` and `net8.0`.
