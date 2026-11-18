# Pricing boundary

> **The model translates. The code prices.** Catalogue items are priced deterministically from the
> price list. A line with no catalogue match receives a labelled model estimate. No model ever
> returns the quote total.

This is the design decision the project is built around, and the axis the thesis measured.
Diagram: [pricing-pipeline.mmd](../diagrams/pricing-pipeline.mmd).

## Why a boundary at all

Input is unstructured Italian written by a homeowner: dimensions, wishes, contradictions, typos.
Output is a commercial document with money on it. Those two languages are far apart, and only one
of them tolerates approximation.

The system splits the problem:

- the model handles language;
- deterministic C# handles quantities, catalogue selection and arithmetic.

## Stage 1 — intent, not price

`OpenAiIntentParsingService` sends the description with a prompt that fixes an output schema:
recipe name, dimensions, a set of scope flags with an optional material preference and quantity
override, detected bundles, free-form custom requests, and apartment-level metadata.

The response is parsed tolerantly (case-insensitive, comments and trailing commas allowed, Italian
decimal separators handled). One cross-field invariant is repaired explicitly: a
`full_renovation` result without its metadata object gets defaults plus a warning. If parsing
throws, the service returns an empty bathroom template with all scope flags false and a warning,
rather than a half-populated quote.

At the end of this stage the model has produced *intent*. It has produced no price.

## Stage 2 — deterministic construction

A recipe implements `IRecipe`. It receives the structured intent and emits line items. Recipes
own:

- geometry (area, perimeter, wall area, derived quantities);
- scope isolation (a bathroom flag cannot add a facade item);
- waste factors;
- the choice of catalogue code for each line it understands;
- deduplication and completeness constraints;
- removal of items that do not belong to the renovation type.

Recipes do not contain prices. They contain catalogue codes and quantities. Prices live in the
price-list table, imported from CSV.

## Stage 3 — resolution and its three outcomes

Every line ends in one of three states:

| Outcome | What happened | What the quote shows |
|---|---|---|
| Catalogue hit | A known code resolved against the price list | Catalogue description and unit price |
| Reranked catalogue hit | Candidate entries were retrieved, then the model chose among them and the guards accepted the result | Catalogue description and unit price |
| Model estimate | No candidate survived the guards | A model-estimated unit price, tagged `AI_ESTIMATE`, and a visible label in the UI and PDF |

The same resolution service, `VectorPriceListService`, performs the lookup regardless of whether
the recipe supplied a code. Direct-code lookups do not call a model. Unknown items go through
candidate search and a model rerank; the reranker's job is to pick one of the candidates, not to
invent a number.

Before a candidate is accepted, guards run:

- a high-confidence semantic score can bypass the reranker;
- a material-integrity check rejects a close substitute when the user named a specific material
  (the marble-versus-gres case);
- a budget sanity check rejects a catalogue price far above the task's stated ceiling, which sends
  the line to the estimate fallback.

The semantic candidate search is backed by Qdrant in local development. No vector store is
provisioned by the deployment at this snapshot; see [limitations.md](limitations.md).

## Stage 4 — totals

`QuoteItem.TotalPrice` is `Quantity * UnitPrice`. `Quote.TotalPrice` is the sum of item totals.
Both are `decimal`; the database columns are `decimal(18,2)`, including the price list's unit
price. The total is computed on every read; it is not stored.

VAT is applied as a rate on the quote and rendered by the PDF. The API does not let a model
produce either number.

## Failure modes and blast radius

| Failure | Effect | Containment |
|---|---|---|
| Model invents scope not requested | Extra line items | Scope flags are explicit; recipes only emit for active flags; operator can edit |
| Model returns a wrong quantity | Wrong line total | Quantity caps and geometry constraints in recipes; the thesis measured this |
| Unknown material | No catalogue match | Rerank, then labelled estimate; the operator sees the label |
| Model returns malformed JSON | No quote | Tolerant parse, invariant repair, safe empty fallback with warning |
| Model returns a bad catalogue choice | Wrong unit price | Guards reject implausible candidates; operator review before sending |

The important property is not that the model is always right. It is that a model error is
bounded: it can add, omit or misprice individual lines, and it can never overwrite the total
algorithm.

## What the thesis measured

The evaluation in Chapter 4 compared early iterations with later ones across 42 estimates,
checking geometric consistency, price fairness and scope correctness. Moving computation from the
model to C# reduced the kitchen total-cost deviation from 39% to 0% and removed €1,822.83 of
erroneous cost. That evaluation used the same estimates for tuning; there is no holdout set.
See [evaluation.md](evaluation.md).

The model-estimate fallback was not separately measured. It is visible in the UI, labelled, and
outside the thesis's deterministic claim.

## Redaction note

No price-list row, catalogue code, supplier name or real unit price appears in this repository.
Examples elsewhere are synthetic.
