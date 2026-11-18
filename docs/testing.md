# Testing

## Framework

- **xUnit** for tests.
- **Moq** for handler and service doubles.
- The test project targets the same `net9.0` runtime and references the application and
  infrastructure projects; the domain comes in transitively.

## Status at the snapshot

| Fact | Value |
|---|---|
| Branch | `test/autopreventivo-xunit` |
| Location | `tests/AutoPreventivo.Tests` |
| Test methods | 6 |
| Merged into `main` | No |
| Run by CI | No |
| Test project tracked on `main` | No (branch-only) |

The GitHub Actions workflow builds and deploys the API. It has no test stage. Claiming continuous
integration for tests would be false: the suite is a local branch, not part of the pipeline.

## What is covered

| Test | What it pins |
|---|---|
| CAPTCHA failure stops generation | A failed token returns the failure and never calls the quote generator |
| Recipe name from the model wins | A parsed recipe overrides the request's default, and the default is never executed |
| Item total equals quantity × unit price | The line arithmetic |
| Quote total is the sum of line totals | Composition of line totals |
| Empty quote totals zero | Degenerate case |
| Exact total from a known intent and catalogue | A six-line quote totals to the cent, and the semantic-search path is never invoked when every line has a known code |

The last test is the important one for this project: it asserts both the money and the boundary.
It fails if a change makes a catalogue-priced line wander into model-assisted resolution.

Snippet: [04-exact-total-test.cs](../snippets/04-exact-total-test.cs). Catalogue codes and prices
in that snippet are synthetic.

## What is not covered

- No integration tests against a real database.
- No HTTP-level tests of the endpoints, auth headers or CAPTCHA behaviour.
- No tests for PDF layout or blob upload.
- No tests for the model parser, reranker or estimate fallback.
- No frontend tests.
- No load, volume or concurrency tests.
- No regression suite for the model routes.

The suite is a start, not a safety net. That is stated here because the case study is evidence,
not marketing.

## Why the branch was not merged at the snapshot

The tests were written during a review pass and kept on a dedicated branch. Merging them and
adding a CI test stage are listed as next steps. Neither is claimed as done.

## Running them (not required for this repository)

This repository is documentation only. The commands below describe the private workflow, not
something a reader can run here.

```bash
# In the private backend repository, on the test branch:
dotnet test tests/AutoPreventivo.Tests/AutoPreventivo.Tests.csproj
```

No command in this case study was executed against the application; see [SYNC.md](../SYNC.md).
