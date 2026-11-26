# API contract

Representative endpoints only. Six of sixteen are documented here; the remainder are admin
quote-list, admin-key-check, price-list, import and maintenance routes, plus a stub test endpoint.
They are omitted deliberately. All values below are synthetic.

## Authentication summary

| Scheme | Where | Behaviour |
|---|---|---|
| reCAPTCHA token | `POST /api/quotes/generate` | Verified in the handler before any work; failure returns `400` |
| `X-Admin-Key` | Admin routes | One fail-closed endpoint filter; a missing or empty configured key, a missing header or a wrong key returns `401` |
| None | Public retrieval and PDF | The quote GUID is the identifier |

There is no bearer token, cookie session or OAuth flow.

---

## 1. Create a quote from free text

`POST /api/quotes/generate` — public, reCAPTCHA required.

Request:

```json
{
  "recaptchaToken": "synthetic-token",
  "clientName": "Giulia Bianchi",
  "clientEmail": "giulia.bianchi@example.test",
  "clientPhone": "+39 000 000 0000",
  "clientAddress": "Via Esempio 1, Pisa",
  "prompt": "Ristrutturare il bagno di 5 mq, sostituire sanitari e piastrelle, 2 lavabi."
}
```

Response `200` (abridged; `items` shortened):

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "title": "Preventivo: Ristrutturazione Bagno",
  "origin": "AI",
  "isDraft": false,
  "clientName": "Giulia Bianchi",
  "vatRate": 0.10,
  "items": [
    {
      "position": 0,
      "code": "EXAMPLE-001",
      "category": "Demolizioni",
      "description": "Demolizione rivestimenti",
      "unit": "mq",
      "quantity": 9.00,
      "unitPrice": 12.00,
      "totalPrice": 108.00
    },
    {
      "position": 1,
      "code": "EXAMPLE-002",
      "category": "Sanitari",
      "description": "Fornitura e posa lavabo",
      "unit": "cad",
      "quantity": 2.00,
      "unitPrice": 150.00,
      "totalPrice": 300.00
    }
  ],
  "totalPrice": 408.00,
  "pdfUrl": "https://storage.example.test/quotes/00000000-0000-0000-0000-000000000001.pdf?<sas>",
  "createdAt": "2026-01-01T10:00:00Z",
  "warnings": []
}
```

The structured-input variant uses `"recipeName"` and `"parameters"` instead of `"prompt"`. It
skips model parsing but still runs a recipe and the same pricing pipeline.

---

## 2. Retrieve a quote

`GET /api/quotes/{id}` — public. Returns the same quote shape as above. A malformed or unknown
GUID returns `404`.

The GUID is the only access control on this endpoint. It is not enumerable in practice, but it is
not an authorisation boundary either.

---

## 3. Download the quote PDF

`GET /api/quotes/{id}/pdf` — public.

- `200` with `Content-Type: application/pdf` and a filename derived from the quote ID.
- `404` with `{ "error": "Quote not found" }` when the ID is unknown.

The PDF is regenerated from the stored quote on each call. The blob copy and its SAS URL are a
separate, durable artefact.

---

## 4. Create a manual quote

`POST /api/quotes/manual` — `X-Admin-Key` required.

Request:

```json
{
  "title": "Rifacimento facciata — esempio",
  "clientName": "Mario Esempio",
  "clientEmail": "mario.esempio@example.test",
  "clientPhone": "+39 000 000 0000",
  "clientAddress": "Via Esempio 2, Pisa",
  "vatRate": 0.10,
  "items": [
    {
      "description": "Ponteggio fisso",
      "category": "Allestimento",
      "unit": "mq",
      "quantity": 100,
      "unitPrice": 5.00
    }
  ],
  "internalNotes": "Creato con CreaTivo (Manuale)",
  "publicNotes": "Prezzi IVA esclusa.",
  "isDraft": false
}
```

- `200` with the created `Quote` (`origin: "Manual"`, items tagged `MANUAL_ENTRY`).
- `400` with `ValidationProblemDetails` and field errors when the payload fails
  `CreateManualQuoteValidator`; the request is not persisted.
- `401` when the admin key is missing or wrong.

Validation runs through the MediatR `ValidationBehavior`. The response includes a
`correlationId` matching the `X-Correlation-ID` header, and the invalid request never reaches
the handler or repository.

With `"isDraft": true`, the response has `pdfUrl: null` and no blob write occurs.

---

## 5. Update a manual quote

`PUT /api/quotes/{id}` — `X-Admin-Key` required.

Same request body as creation. The handler replaces the item collection, tags the new items
`MANUAL_UPDATE`, and regenerates the PDF only when the quote is not a draft.

- `200` with the updated `Quote`.
- `400` with `{ "code": "Quote.NotFound", "name": "Preventivo non trovato." }` for an unknown ID.
  The route reports a missing quote as a bad request rather than a 404.
- `401` on a bad key.

Update is **not validated**. `UpdateQuoteCommand` uses the same request DTO but has no
FluentValidation validator, so field ranges are enforced only on creation.

---

## 6. Read the catalogue

`GET /api/pricelist` — `X-Admin-Key` required. This is the lookup/config endpoint used by the
price-list management page.

Response `200` (abridged; one synthetic row shown):

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000101",
    "code": "EXAMPLE-101",
    "category": "Demolizioni",
    "fullDescription": "Demolizione rivestimenti",
    "unit": "mq",
    "price": 12.00,
    "actionType": "SUPPLY_AND_INSTALL",
    "tags": "demolizione, rivestimenti",
    "dimension": 2,
    "chapterId": "01"
  }
]
```

- `200` with the full catalogue.
- `401` when the admin key is missing or wrong.

The catalogue is also exposed through admin create, update and delete routes, a search route used
by the quote builder, a CSV upload route and a CSV import route, all omitted here.

---

## Deliberately omitted

The remaining ten endpoints cover the admin quote list, the admin key check, a stub test route,
price-list search and management, CSV upload, CSV import, and one Qdrant maintenance operation.
They are not documented because they either duplicate the selected behaviour or expose
catalogue-maintenance internals of the commercial system.
