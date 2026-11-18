# Sync

This case study describes private repositories at a fixed point. It is not a mirror and does not
track commits.

## Snapshot

| Item | Value |
|---|---|
| Private backend | `edilvos-platform` |
| Backend branch | `main` |
| Backend commit | `2dee6ae72c10c892b7354359b0ff8fd3ae512ec0` |
| Backend commit date | 2026-05-30 |
| Private frontend | `edilvos` |
| Frontend branch | `main` |
| Frontend commit | `bc9a48e6cfbbf96b7904841923f2b14c02216bdf` |
| Frontend commit date | 2026-02-28 |
| Case-study snapshot date | 2026-11-18 |

The frontend working tree had two pre-existing modified files at snapshot time
(`app/(site)/preventivo/page.tsx`, `components/QuoteGenerator.tsx`). They were not read into the
case study beyond what the committed history and the static inspection showed.

## Scope

Included: architecture, request flow, both quote paths, pricing boundary, data model, six
representative endpoints, testing status, thesis evaluation, limitations, three Mermaid diagrams
and six reviewed snippets.

Excluded by design: application source, solution files, package manifests, Docker Compose,
workflows, configuration values, price-list CSVs, database content, PDFs, customer data,
screenshots with real data, and any Azure resource identifier.

## What the snapshot does not establish

- **The deployed version is not verified.** The commits above are local `main` HEADs, not proof of
  what App Service or Vercel currently serves.
- **Runtime configuration is not visible.** The model endpoint, storage account and database the
  live service uses are deployment-side settings.
- **Production volume is owner-reported.** 5–30 quotes/month is not independently measured.

## How to refresh

At a milestone, not per commit:

1. Re-run a read-only inspection of the private repositories.
2. Update this file with the new commit hashes and date.
3. Re-check every claim that changed; remove anything no longer true.
4. Re-run the redaction checklist before publishing an update.

No automated sync, submodule or remote points at the private repositories.
