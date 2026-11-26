# Testing

## Framework

- **xUnit** for tests.
- **Moq** for handler and service doubles.
- **`WebApplicationFactory`** for in-process endpoint tests; no real HTTP listener, database or
  external service is used.
- The test project targets `net10.0` and references the application and infrastructure projects;
  the domain comes in transitively.

## Status at the snapshot

| Fact | Value |
|---|---|
| Branch | `main` |
| Location | `tests/AutoPreventivo.Tests` |
| Tests | 48 |
| Merged into `main` | Yes |
| Run by CI | Yes — test job on pull requests and `main`; deploy depends on it |
| Snapshot commit | `bd8bf3c` |

The GitHub Actions workflow runs the test project before deployment. A failed test prevents the
deploy job from running. Deployment itself happens only on `main`.

## What is covered

| Area | What it pins |
|---|---|
| Exact totals | Six-line quote totals to the cent; five renovation recipes produce their expected totals |
| Pricing boundary | Catalogue-priced lines never enter the semantic/model path; the exact-total test fails if that boundary moves |
| Fallback labelling | Unmatched lines receive and retain the `AI_ESTIMATE` tag; missing price-list codes are reported |
| Parser failure modes | Unknown recipe guard, full-renovation invariant repair, malformed model JSON, Italian decimal comma |
| Degenerate arithmetic | Empty quote totals zero; decimal precision and non-positive quantity boundaries |
| Manual-quote validation | Invalid payload returns 400 `ValidationProblemDetails` with field errors and does not reach the repository; a valid payload does |
| Admin access control | Every admin route returns 401 without a key; a wrong key returns 401; a missing configured key fails closed; public routes stay public |
| Health and correlation | `/health` returns 200; a correlation ID is echoed, generated when absent, and replaced when invalid; Serilog is the logger factory |

The exact-total test remains the important one for this project: it asserts both the money and the
boundary.

Snippet: [04-exact-total-test.cs](../snippets/04-exact-total-test.cs). Catalogue codes and prices
in that snippet are synthetic.

## What is not covered

- No integration tests against a real database, Azure SQL, Blob Storage, Qdrant or OpenAI.
- No tests for PDF layout or real blob upload.
- No browser tests for the frontend.
- No load, volume or concurrency tests.
- No regression suite for the model routes beyond parser failure modes.
- No test proves the deployed environment matches the repository.

The suite is a real gate for the code paths it covers, not a safety net for the production
environment.

## Running them (not required for this repository)

This repository is documentation only. The commands below describe the private workflow, not
something a reader can run here.

```bash
# In the private backend repository, on main:
dotnet test tests/AutoPreventivo.Tests/AutoPreventivo.Tests.csproj -c Release
```

Expected: 48/48 passed. No command in this case study was executed against the application; see
[SYNC.md](../SYNC.md).
