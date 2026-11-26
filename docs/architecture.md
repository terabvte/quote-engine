# Architecture

Snapshot: private backend `bd8bf3c`, 2026-11-26. Backend deployment was smoke-tested; see
[SYNC.md](../SYNC.md).

## System context

The product is a single ASP.NET Core API with one Next.js frontend. There is no microservice
split, no message broker and no separate worker process.

| Component | Responsibility |
|---|---|
| Next.js frontend | Marketing pages, the public quote form, the admin dashboard. Deployed on Vercel. |
| `AutoPreventivo.Api` | The product's HTTP surface: 16 API endpoints plus `/health`, Minimal APIs, .NET 10. |
| `AutoPreventivo.Application` | Commands, queries and handlers (MediatR). FluentValidation validators are registered from the product assembly; `CreateManualQuoteValidator` is wired into the MediatR pipeline. |
| `AutoPreventivo.Domain` | Entities, six renovation recipes, the price-list contract, computation rules. |
| `AutoPreventivo.Infrastructure` | EF Core 10 against SQL Server/Azure SQL, the OpenAI clients, QuestPDF, Azure Blob Storage, the Qdrant-backed price-list service. |
| `Edilvos.Api` / `Edilvos.Application` / `Edilvos.Domain` / `Edilvos.Infrastructure` | A small shared shell. The deployed API is `AutoPreventivo.Api`; the shell provides shared abstractions such as `Result`. |
| Azure SQL | Stores `Quotes`, `QuoteItems` and `PriceListEntries`. |
| Azure Blob Storage | Stores generated PDFs in a `quotes` container; uploads return a 24-hour read SAS URL. |
| OpenAI API | Parses free-text intent, reranks catalogue candidates, and estimates a unit price when no catalogue match survives the guards. |
| GitHub Actions | Restores, builds and tests the API; deploys to Azure App Service on pushes to `main` under `src/**`. Deployment depends on the test job. |

Diagram: [system-context.mmd](../diagrams/system-context.mmd).

## Solution layout

Eight projects under `Edilvos.Platform.sln`, all targeting `net10.0`:

```
src/Edilvos.Api
src/Edilvos.Application
src/Edilvos.Domain
src/Edilvos.Infrastructure
src/services/AutoPreventivo/AutoPreventivo.Api
src/services/AutoPreventivo/AutoPreventivo.Application
src/services/AutoPreventivo/AutoPreventivo.Domain
src/services/AutoPreventivo/AutoPreventivo.Infrastructure
```

The Edilvos layer is a shell, not a second product. `Edilvos.Application` and `Edilvos.Domain`
provide shared types. The five `docs/learn/experiments` projects remain on `net9.0` by decision
and are not part of the deployable path.

## Scale

| Measure | Value | How it was established |
|---|---|---|
| Renovation recipes | 6 | Six `IRecipe` implementations |
| HTTP endpoints | 16 plus `/health` | `Map*` calls in `AutoPreventivo.Api/Program.cs` |
| EF Core migrations | 18 | Migration files, excluding designers and the model snapshot |
| Price-list rows | 121 | Three CSV imports: 43 + 42 + 36 data rows |
| Quote paths | 2 | AI generation, manual CreaTivo |
| Tests | 48 | xUnit, in-process only; CI test job gates deployment |
| Production volume | 5–30 quotes/month | Owner-reported; not independently measured |

## Request path

1. The browser posts to the Next.js server action, which forwards JSON to the API.
2. The API handler verifies the reCAPTCHA token.
3. The intent parser calls the model with a schema-shaped prompt and repairs cross-field
   invariants.
4. A deterministic recipe turns the structured intent into line items, quantities, waste factors
   and price-list candidates.
5. Known catalogue codes are resolved directly. Unknown items go through candidate search and a
   model rerank; if that fails, a model-estimated unit price is attached and labelled.
6. Totals are computed in C# from `decimal` values.
7. QuestPDF renders the PDF, which is uploaded to Blob Storage; the quote is persisted with the
   blob URL.

Full flow: [request-flow.md](request-flow.md). Pricing boundary: [pricing-boundary.md](pricing-boundary.md).

## Authentication and authorisation

- **Admin endpoints:** a shared secret in the `X-Admin-Key` header. One
  `AdminKeyEndpointFilter` guards all 11 admin routes. A missing or empty configured key fails
  closed. Comparison hashes both sides with SHA-256 and uses
  `CryptographicOperations.FixedTimeEquals`. There is no identity, role model, per-user audit,
  rotation or expiry.
- **Public quote generation:** reCAPTCHA is verified inside the handler before any model call or
  database write.
- **Public quote retrieval and PDF download:** no authentication; the quote GUID is the only
  identifier.
- **Admin UI:** the key is entered in the browser and kept in `localStorage`, then sent to a
  Next.js server action as a header.
- **Health:** `GET /health` is public and liveness-only. It checks no external dependency.

## Validation

- `POST /api/quotes/manual` runs `CreateManualQuoteValidator` through the MediatR
  `ValidationBehavior`.
- A `ValidationExceptionHandler` maps FluentValidation failures to `400 ValidationProblemDetails`
  with field errors and a `correlationId`; the invalid request never reaches the repository.
- `PUT /api/quotes/{id}` uses the same DTO but has no validator and remains unvalidated.
- Data annotations on the request record are not evaluated by Minimal APIs.

## Observability

- Serilog structured logging with a compact JSON console sink.
- Application Insights request and trace telemetry, with adaptive sampling. The connection string
  is supplied by App Service configuration; no value is in source.
- Correlation IDs: `X-Correlation-ID` is accepted when valid, otherwise the Kestrel trace
  identifier is used; the response echoes it and it is attached to log events.
- `UseSerilogRequestLogging` runs outside `UseExceptionHandler`, so handled validation failures
  log as 400.
- No alert rules, dashboards or dependency-health checks are configured.

Not implemented: Entra ID, Managed Identity, Key Vault, OAuth/OIDC, application roles, key
rotation, request signing, rate limiting. None of these are claimed.

## Deployment

- `.github/workflows/deploy-backend.yaml` runs a test job on pull requests and on pushes to
  `main` under `src/**`.
- The test job restores, builds and runs the 48-test project on .NET 10.
- `build-and-deploy` depends on the test job and is skipped on pull requests.
- Stages: checkout, .NET 10 setup, restore, release build, publish, `azure/login@v2` with a
  credential secret, `azure/webapps-deploy@v3` to the App Service named by a repository variable.
- The workflow declares `id-token: write` but authenticates with a credential secret, not OIDC.
- There is no Dockerfile and the application is not containerised.
- The App Service plan is F1 Free with no deployment slot.
- `docker-compose.yml` is local development only: SQL Server 2022, Azurite and Qdrant.

## Configuration

The API reads configuration by key; no values are reproduced here. Areas that must be supplied at
runtime: the SQL connection string, the admin access key, the OpenAI API key, the Blob Storage
connection string, the Application Insights connection string, and a Qdrant host. Qdrant
defaults to `localhost` when unset.

## External services and their status

| Service | Status at snapshot |
|---|---|
| OpenAI API | Active in code. The backend uses the OpenAI .NET SDK with an API key and no Azure endpoint. Moving to Azure OpenAI is a planned step, not a current claim. |
| Azure Blob Storage | Active in code; Azurite locally. |
| Azure SQL / SQL Server | Active in code; SQL Server locally. |
| reCAPTCHA | Active on the public quote endpoint. |
| Application Insights | Active in production; requests and traces confirmed after deployment. No alerts or dashboards. |
| Qdrant | Wired as the default price-list search implementation, but no Qdrant resource is provisioned by the deployment. Exercised in local development only. Do not read the diagrams as claiming a deployed vector store. |
| Resend | Package and service class exist, but the registration and send calls are commented out. Email notification is not active. |
