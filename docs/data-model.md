# Data model

Three persisted entities. No price-list rows, customer records or quote examples are reproduced
in this repository.

## Quote

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Generated in the domain, used in public URLs |
| `Title` | `string(200)` | Human-readable title |
| `Origin` | `string(50)` | `"AI"` or `"Manual"` |
| `IsDraft` | `bool` | Manual-path drafts skip PDF generation |
| `ClientName` | `string` | Customer name |
| `ClientEmail`, `ClientPhone`, `ClientAddress` | `string?` | Optional contact fields |
| `VatRate` | `decimal` | Stored as a fraction on the quote |
| `InternalNotes`, `PublicNotes` | `string?` | Internal vs printed notes |
| `PdfUrl` | `string?` | Blob URL with a time-limited SAS |
| `CreatedAt` | `DateTime` | UTC |
| `Items` | `ICollection<QuoteItem>` | One-to-many |
| `TotalPrice` | computed `decimal` | Sum of item totals; not stored |

Also present but not persisted: `Warnings` (recipe and parser warnings returned with the quote)
and `VectorSearchLog` (per-request record of resolution calls).

## QuoteItem

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | EF-generated |
| `QuoteId` | `Guid` | Foreign key |
| `Position` | `int` | Display order |
| `Code` | `string(50)` | Catalogue code, `SEARCH_NEEDED`, `AI_ESTIMATE`, or `MANUAL_ENTRY`/`MANUAL_UPDATE` |
| `Category` | `string(100)` | Groups lines in the PDF |
| `Description` | `string` (unbounded) | Printed description |
| `Unit` | `string` (unbounded) | `mq`, `mc`, `cad`, `corpo`, `ml` |
| `Quantity` | `decimal(18,2)` | |
| `UnitPrice` | `decimal(18,2)` | Catalogue price, operator price, or labelled estimate |
| `TotalPrice` | computed `decimal` | `Quantity * UnitPrice`; not stored |

Non-mapped fields carry search hints between the recipe and the resolver: chapter, target action,
thickness, forbidden terms, minimum and maximum price. They are request-scoped and never reach
the database.

## PriceListEntry

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Generated |
| `Code` | `string(50)` | Catalogue code, unique by lookup |
| `Category` | `string(100)` | Trade category |
| `FullDescription` | `string(4000)` | Printed description |
| `Unit` | `string(20)` | Unit of measure |
| `Price` | `decimal(18,2)` | Catalogue unit price |
| `ActionType` | `string(50)` | `SUPPLY_ONLY`, `INSTALL_ONLY`, `SUPPLY_AND_INSTALL` |
| `Tags` | `string` | Search terms |
| `Dimension` | `int` | 1 linear, 2 area, 3 volume, 0 item |
| `ChapterId` | `string(10)` | Chapter grouping |

The table is populated from three CSV imports: 43, 42 and 36 data rows, 121 in total. The CSVs
themselves are commercial material and are not in this repository.

## Relationships

```
Quote 1 ─── * QuoteItem
PriceListEntry        (independent; read by code or by search)
```

There is no customer entity, no user entity, no audit entity and no payment entity. Client
details live on the quote. Admin identity is a shared key, not a row.

## Precision and rounding

- Quantities, line unit prices and catalogue unit prices are all stored as `decimal(18,2)`.
- The entity attribute on `PriceListEntry.Price` says `decimal(18,5)`, but the EF Core fluent
  configuration sets `decimal(18,2)` and wins; the model snapshot and migrations use 18,2.
- Totals are computed in C# on each read; no rounded total is persisted.
- VAT is a rate on the quote; the PDF renders the arithmetic.

Changing any of these is a domain change, not a formatting change. The case study documents the
current behaviour, not a recommendation.
