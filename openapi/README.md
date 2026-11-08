# The API contract this SDK is tested against

`thrivea-v1.openapi.json` is the Thrivea public API's OpenAPI document, as the backend exports it
(`make public-api-openapi-export` in `thrivea-backend`, from `docs/app/specs/public-api/openapi/`). It is copied here,
not referenced, so a build of this repository needs nothing outside it.

| | |
|---|---|
| Upstream | `thrivea/thrivea-backend`, `docs/app/specs/public-api/openapi/thrivea-v1.openapi.json` |
| Backend commit | `67619f42dc` |
| Exported | `2026-11-08` |
| sha256 | `7271dc799d91d32f4ecc4c2d1a92b088e82abb8ce008e5c9d0da9ee8ff60bdf7` |

`OpenApiConformanceTests` checks the SDK against it: every route a service calls exists, every field a model reads
or writes is in its schema and every schema field is known to a model, writes declare `Idempotency-Key`, every
answer declares `Request-Id` and the `RateLimit-*` headers, and the integer enums agree. A red test there means
the API and the SDK have parted; the fix is a new SDK version (or an entry in `NotYetSurfaced` with a reason), never
an edit of this file by hand.

To update: re-export in the backend, copy the file here, fill the table above, and run the tests. A nightly workflow
(`openapi-drift.yml`) compares this copy with what the dev host serves and opens an issue when they differ.
