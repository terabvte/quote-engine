# Snippets

Six short excerpts, adapted from the private repositories. Each one is a review artefact, not a
copy of a file: comments, unrelated branches and notification code were removed, and every
catalogue code and price visible in the snippets is synthetic.

| File | Source (private) | Why it is here |
|---|---|---|
| `01-generate-quote-handler.cs` | `AutoPreventivo.Application/Features/Quotes/GenerateQuote/GenerateQuoteHandler.cs` | The whole AI path in one screen: security check, intent parsing, recipe execution, PDF, blob upload, persistence. Shows that the handler orchestrates and does not price. |
| `02-create-manual-quote-handler.cs` | `AutoPreventivo.Application/Features/Quotes/CreateManual/CreateManualQuoteHandler.cs` | The CreaTivo path: manual items, `Origin = "Manual"`, draft short-circuit, reuse of the same PDF/blob/repository services. |
| `03-intent-invariant-repair.cs` | `AutoPreventivo.Infrastructure/Services/OpenAiIntentParsingService.cs` | The post-parse boundary: tolerant deserialisation, one explicit cross-field repair, and a safe empty fallback instead of a partial quote. |
| `04-exact-total-test.cs` | `tests/AutoPreventivo.Tests/Pricing/DeterministicPricingTests.cs` (branch `test/autopreventivo-xunit`) | Pins a total to the cent and asserts the semantic path is never called when every item has a catalogue code. Codes and prices are synthetic. |
| `05-create-manual-quote-action.ts` | `app/actions/create-manual-quote.ts` (frontend) | Server-action boundary: nested UI state flattened into the API contract, one action for create and update, admin key passed as a header. |
| `06-provenance-tags.tsx` | `components/QuoteGenerator.tsx` (frontend) | The UI side of the pricing boundary: lines carrying a model estimate are detected and labelled before rendering. |

## What was deliberately not included

- Recipe bodies, waste factors, scope matrices and the renovation manifest.
- Real price-list codes, descriptions, unit prices and CSV rows.
- The full model prompt and its routing rules.
- Any file large enough to be a source copy rather than an illustration.
