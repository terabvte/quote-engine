# Sync

This case study describes private repositories at a fixed point. It is not a mirror and does not
track commits.

## Snapshot

| Item | Value |
|---|---|
| Private backend | `edilvos-platform` |
| Backend branch | `main` |
| Backend commit | `bd8bf3c172e74335ac9cb45541406a67e80346b9` |
| Backend commit date | 2026-11-26 |
| Private frontend | `edilvos` |
| Frontend branch | `main` |
| Frontend commit | `bc9a48e6cfbbf96b7904841923f2b14c02216bdf` |
| Frontend commit date | 2026-02-28 |
| Case-study refresh date | 2026-11-26 |

The frontend working tree had two pre-existing modified files at snapshot time
(`app/(site)/preventivo/page.tsx`, `components/QuoteGenerator.tsx`). They were not read into the
case study beyond what the committed history and the static inspection showed.

The backend snapshot now includes Phases 3–5: the merged test suite and CI gate, request
validation, the centralised admin-key filter, observability, and the .NET 10 / EF Core 10
upgrade.

## Scope

Included: architecture, request flow, both quote paths, pricing boundary, data model, six
representative endpoints, testing status, thesis evaluation, production hardening, three Mermaid
diagrams and six reviewed snippets.

Excluded by design: application source, solution files, package manifests, Docker Compose,
workflows, configuration values, price-list CSVs, database content, PDFs, customer data,
screenshots with real data, and any Azure resource identifier.

## What the snapshot does not establish

- **Backend deployment was smoke-tested, not exhaustively verified.** After the `bd8bf3c`
  deployment, `/health` returned 200, an admin request without a key returned 401, malformed JSON
  returned 400, and an unknown quote ID returned 404. Other routes and background behaviour were
  not exercised.
- **The frontend deployment is not verified.** Package deployment on Vercel is outside this
  repository.
- **Runtime configuration is not visible.** The model endpoint, storage account, database and
  Application Insights resource the live service uses are deployment-side settings.
- **Production volume is owner-reported.** 5–30 quotes/month is not independently measured.
- **Tests are in-process.** They use mocks and a `WebApplicationFactory` TestServer, not real
  SQL Server, Azure SQL, Blob Storage, Qdrant or OpenAI endpoints.

## How to refresh

At a milestone, not per commit:

1. Re-run a read-only inspection of the private repositories.
2. Update this file with the new commit hashes and date.
3. Re-check every claim that changed; remove anything no longer true.
4. Re-run the redaction checklist before publishing an update.

No automated sync, submodule or remote points at the private repositories.
