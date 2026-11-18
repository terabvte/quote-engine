# Request flow

Two creation paths converge on the same quote entity, PDF generator, blob storage and database.
Both are shown in [request-flow.mmd](../diagrams/request-flow.mmd).

## AI path — AutoPreventivo

1. A visitor opens the quote form on `edilvos.it`. The form accepts a free-text Italian
   description, or precise dimensions in a structured mode.
2. The browser obtains a reCAPTCHA v3 token. The Next.js server action validates the input and
   posts it to `POST /api/quotes/generate` with the client's name, email, phone and address.
3. `GenerateQuoteHandler` verifies the reCAPTCHA token before doing anything else. A failed token
   ends the request without a model call or database write.
4. `OpenAiIntentParsingService` sends the text to the model with a schema-shaped prompt. The
   response is parsed tolerantly, one cross-field invariant is repaired (`full_renovation`
   without its metadata object), and a safe empty fallback is returned if parsing throws.
5. The structured intent names a recipe. The handler normalises the name and asks
   `IQuoteGeneratorService` to run it.
6. The recipe computes quantities, scope isolation and waste factors, and emits line items. Each
   item either carries a known catalogue code or is marked for search.
7. Pricing resolves per item:
   - known code → direct catalogue lookup;
   - unknown code → candidate search, then a model rerank among candidates, then sanity guards;
   - no surviving candidate → a model-estimated unit price, tagged `AI_ESTIMATE`.
8. `Quote.TotalPrice` and `QuoteItem.TotalPrice` compute decimals in C#. No model returns a total.
9. QuestPDF renders the PDF. `AzureBlobStorageService` uploads it and returns a 24-hour read SAS
   URL, which is stored on the quote.
10. `IQuoteRepository` persists the quote and its items. The API returns the quote as JSON; the
    form renders the line items, highlights any model-estimated line, and offers the PDF.

The model is not asked for prices on the catalogue path, and never for a total. The fallback in
step 7 is the one place where a model-supplied number can enter a quote, and it is labelled.
See [pricing-boundary.md](pricing-boundary.md).

## Manual path — CreaTivo

1. An operator opens the admin area and enters the shared admin key. The browser keeps it in
   `localStorage` and sends it as `X-Admin-Key` on every admin request.
2. The CreaTivo builder (`/admin/creativo`) holds client details, VAT rate, notes, and ordered
   sections of line items. The operator adds, edits and reorders items; totals update live.
3. **Save draft:** the server action posts to `POST /api/quotes/manual` with `IsDraft = true`.
   `CreateManualQuoteHandler` persists the quote with `Origin = "Manual"` and
   `QuoteItem.Code = "MANUAL_ENTRY"`, skips PDF generation and leaves `PdfUrl` null.
4. **Save final:** the same endpoint with `IsDraft = false` generates the PDF with the same
   `IPdfGeneratorService`, uploads it with the same `IBlobStorageService`, and persists with the
   same `IQuoteRepository`.
5. **Edit:** the builder loads `GET /api/quotes/{id}` and saves with `PUT /api/quotes/{id}`.
   `UpdateQuoteHandler` replaces the item collection, tags the new items `MANUAL_UPDATE`, and
   regenerates the PDF only when the quote is final.
6. No model call exists in this path.

## Shared behaviour

| Concern | AI path | Manual path |
|---|---|---|
| Quote entity | `Quote` | `Quote` |
| Origin | `"AI"` | `"Manual"` |
| Item code | Catalogue code, `SEARCH_NEEDED`, or `AI_ESTIMATE` | `MANUAL_ENTRY` / `MANUAL_UPDATE` |
| Draft support | No | Yes |
| PDF | QuestPDF | Same QuestPDF service |
| Storage | Blob container `quotes` | Same container and service |
| Persistence | `IQuoteRepository` | Same repository |
| Download | `GET /api/quotes/{id}/pdf` | Same endpoint |

The download endpoint regenerates the PDF from the quote on each request rather than redirecting
to the stored blob. The blob copy is a durable artefact with a time-limited SAS URL; the endpoint
is the stable path.

## Failure and safety behaviour visible in the flow

- CAPTCHA failure stops the request early.
- Model parsing failure yields an empty template with a warning, not a partial price.
- A catalogue code that cannot be resolved produces a warning on the quote.
- The final safety net in the kitchen recipe removes indoor/bathroom items that leak into a
  kitchen quote through search.
- Draft saves avoid both PDF generation and storage writes.
