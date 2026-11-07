# Changelog

All notable changes to `Thrivea.Sdk` are recorded here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the package follows [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.1.0] - 2026-11-07

### Added

- `ThriveaClient` with Bearer API-key authentication, per-request options, a configurable host and timeout, and retries with exponential backoff that honour `Retry-After`.
- Services for every `/v1` resource: `JobOpenings`, `CandidateApplications` (JSON or multipart with a CV), `Candidates`, `Employees`, `OrgUnits`, `OrgUnitTypes`, `CostCenters`, `WebhookEndpoints`.
- Cursor paging with `ThriveaList<T>` and `ListAutoPagingAsync`.
- `TypeId`: parse, create and check the prefixed identifiers the API uses, with the official specification's test vectors.
- Typed exceptions per error type: `ThriveaInvalidRequestException`, `ThriveaAuthenticationException`, `ThriveaPermissionException`, `ThriveaNotFoundException`, `ThriveaConflictException`, `ThriveaRateLimitException`, `ThriveaApiException`, `ThriveaNetworkException`.
- `WebhookSignature` and `WebhookEvent` for verifying and reading webhook deliveries.
- Targets `netstandard2.0` and `net8.0`.
