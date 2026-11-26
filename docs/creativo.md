# CreaTivo — manual quote creation

CreaTivo is the admin-panel path for quotes that the operator builds by hand: site visits,
phone requests, jobs with no usable free text. It is a feature of the same application, not a
separate service, product or deployment.

## Surface

- Route: `/admin/creativo` in the Next.js frontend.
- Builder: ordered sections containing line items (description, category, unit, quantity, unit
  price). Items can be added, edited, removed and drag-reordered.
- Client details, VAT rate, public notes and internal notes are part of the same form.
- Totals update live in the browser; the authoritative total is still computed by the API from
  decimal values.
- The admin key is entered in the admin area, held in `localStorage`, and sent as `X-Admin-Key`.

## Backend path

| Step | Detail |
|---|---|
| Create | `POST /api/quotes/manual`, `X-Admin-Key` required |
| Update | `PUT /api/quotes/{id}`, `X-Admin-Key` required |
| Command | `CreateManualQuoteCommand` / `UpdateQuoteCommand` |
| Handler | `CreateManualQuoteHandler` / `UpdateQuoteHandler` |
| Validator | `CreateManualQuoteValidator` on create, registered in the MediatR pipeline; no validator on update |
| Origin | `"Manual"` |
| Item codes | `MANUAL_ENTRY` on create, `MANUAL_UPDATE` on update |
| Draft | `IsDraft = true` skips PDF generation; `PdfUrl` stays null |
| PDF | Same `IPdfGeneratorService` (QuestPDF) as the AI path |
| Storage | Same `IBlobStorageService` and `quotes` container |
| Persistence | Same `IQuoteRepository` and database tables |

There is no model call, no intent parsing, no recipe execution and no price-list lookup in this
path. Every quantity and unit price comes from the operator.

## Draft behaviour

A draft is a normal `Quote` row with `IsDraft = true`, `Origin = "Manual"` and no PDF. It appears
in the admin draft tab and can be edited without touching blob storage. Publishing it (saving
without the draft flag) generates the PDF and uploads it. An edit of a final quote regenerates
the PDF under a new blob name, so the previous file is not overwritten.

## What is shared

CreaTivo was built by reusing AutoPreventivo's infrastructure. The quote entity, PDF generator,
blob upload, repository, admin list and PDF download endpoint were already present. The change
was a new command, handler, validator and admin screen — not a new subsystem. Migration
`20260124094741_UpdateQuoteForCreaTivo` added the fields the manual path needed to the quote
model.

## Honest limitations

- Admin authorisation is a shared secret, not an identity. Anyone holding the key has the same
  access as an operator.
- The key lives in browser storage; there is no server-side session or per-user attribution.
- Creation is validated: empty fields and non-positive values return `400` with field errors and
  are not persisted. Updates remain unvalidated, and no check verifies commercial correctness.
