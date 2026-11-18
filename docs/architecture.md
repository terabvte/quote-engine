# Architecture

Snapshot: private backend `2dee6ae`, 2026-05-30. The live deployment may differ; see
[SYNC.md](../SYNC.md).

## System context

The product is a single ASP.NET Core API with one Next.js frontend. There is no microservice
split, no message broker and no separate worker process.

| Component | Responsibility |
|---|---|
| Next.js frontend | Marketing pages, the public quote form, the admin dashboard. Deployed on Vercel. |
| `AutoPreventivo.Api` | The product's HTTP surface: 16 endpoints, Minimal APIs, .NET 9. |
| `AutoPreventivo.Application` | Commands, queries and handlers (MediatR 13). A FluentValidation validator class exists, but the DI scan targets the shell assembly, so it is not wired. |
| `AutoPreventivo.Domain` | Entities, six renovation recipes, the price-list contract, computation rules. |
| `AutoPreventivo.Infrastructure` | EF Core 9 against SQL Server/Azure SQL, the OpenAI clients, QuestPDF, Azure Blob Storage, the Qdrant-backed price-list service. |
| `Edilvos.Api` / `Edilvos.Application` / `Edilvos.Domain` / `Edilvos.Infrastructure` | A small shared shell. `Edilvos.Api` exposes `/health` only; the rest provides shared abstractions such as `Result`. |
| Azure SQL | Stores `Quotes`, `QuoteItems` and `PriceListEntries`. |
| Azure Blob Storage | Stores generated PDFs in a `quotes` container; uploads return a 24-hour read SAS URL. |
| OpenAI API | Parses free-text intent, reranks catalogue candidates, and estimates a unit price when no catalogue match survives the guards. |
| GitHub Actions | Builds and deploys the API to Azure App Service on each push to `main` under `src/**`. |

Diagram: [system-context.mmd](../diagrams/system-context.mmd).

## Solution layout

Eight projects under `Edilvos.Platform.sln`, all targeting `net9.0`:

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

The Edilvos layer is a shell, not a second product. In this snapshot `Edilvos.Api` contains one
endpoint (`/health`); `Edilvos.Application` and `Edilvos.Domain` provide shared types.

## Scale

| Measure | Value | How it was established |
|---|---|---|
| Renovation recipes | 6 | Six `IRecipe` implementations |
| HTTP endpoints | 16 | `Map*` calls in `AutoPreventivo.Api/Program.cs` |
| EF Core migrations | 18 | Migration files, excluding designers and the model snapshot |
| Price-list rows | 121 | Three CSV imports: 43 + 42 + 36 data rows |
| Quote paths | 2 | AI generation, manual CreaTivo |
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

- **Admin endpoints:** a shared secret in the `X-Admin-Key` header, checked inline in the quote
  and price-list handlers against `AdminSettings:AccessKey`. There is no middleware, no global
  policy, no role model and no audit trail.
- **Public quote generation:** reCAPTCHA is verified inside the handler before any model call or
  database write.
- **Public quote retrieval and PDF download:** no authentication; the quote GUID is the only
  identifier.
- **Admin UI:** the key is entered in the browser and kept in `localStorage`, then sent to a
  Next.js server action as a header.

Request validation is not enforced at the edge. A FluentValidation `ValidationBehavior` is
registered by the shell's MediatR setup, but the validator scan points at the shell assembly,
which contains no validators; the product's `CreateManualQuoteValidator` is never resolved. Input
constraints are therefore advisory at this snapshot.

Not implemented at this snapshot: Entra ID, Managed Identity, Key Vault, Bicep, Application
Insights, OAuth/OIDC, application roles, key rotation, request signing, rate limiting. None of
these are claimed.

## Deployment

- `.github/workflows/deploy-backend.yaml` runs on pushes to `main` that touch `src/**`, and can be
  triggered manually.
- Stages: checkout, .NET 9 setup, restore, release build, publish, `azure/login@v2` with
  `secrets.AZURE_CREDENTIALS`, `azure/webapps-deploy@v3` to an App Service named by a repository
  variable.
- The workflow declares `id-token: write` but authenticates with a credential secret, not OIDC.
- There is no test stage.
- There is no Dockerfile and the application is not containerised.
- `docker-compose.yml` is local development only: SQL Server 2022, Azurite and Qdrant.

## Configuration

The API reads configuration by key; no values are reproduced here. Areas that must be supplied at
runtime: the SQL connection string, the admin access key, the OpenAI API key, the Blob Storage
connection string, and a Qdrant host. Qdrant defaults to `localhost` when unset.

## External services and their status

| Service | Status at snapshot |
|---|---|
| OpenAI API | Active in code. The backend uses the OpenAI .NET SDK with an API key and no Azure endpoint. Moving to Azure OpenAI is a planned step, not a current claim. |
| Azure Blob Storage | Active in code; Azurite locally. |
| Azure SQL / SQL Server | Active in code; SQL Server locally. |
| reCAPTCHA | Active on the public quote endpoint. |
| Qdrant | Wired as the default price-list search implementation, but no Qdrant resource is provisioned by the deployment. Exercised in local development only. Do not read the diagrams as claiming a deployed vector store. |
| Resend | Package and service class exist, but the registration and send calls are commented out. Email notification is not active. |
