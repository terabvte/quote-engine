# Screenshot manifest

**Status 2026-11-18: all four screenshots waived by the owner for the first publication.** The
visible placeholder blocks in `README.md` are commented out; the hidden `<!-- SCREENSHOT: ... -->`
markers remain at their insertion points for a later supply.

Owner-supplied screenshots only. Synthetic data. No image file is referenced in the README until
it exists in this folder.

Capture rules: 1440 px browser width, light mode, no browser chrome or tabs, PNG, cropped to
content, no personal data, no real customer prices.

| id | file | section | what to capture | redaction | status |
|---|---|---|---|---|---|
| `quote-form` | `screenshots/quote-form.png` | The problem | Desktop quote form filled with a synthetic Italian request | Fake name, phone, email, address | WAIVED |
| `quote-result` | `screenshots/quote-result.png` | Two quote paths | Generated quote items and total | Fake customer, round amounts | WAIVED |
| `pdf-output` | `screenshots/pdf-output.png` | API | First page of a generated PDF | Fake customer, no real prices | WAIVED |
| `admin-dashboard` | `screenshots/admin-dashboard.png` | Evaluation (end of section) | Admin quote list or detail view | Blur names, emails, phones, addresses | WAIVED |

## Status values

- `TODO` — not supplied.
- `SUPPLIED` — file present in `screenshots/`, checked against the redaction column.
- `WAIVED` — owner explicitly excluded it from this publication.

When a file is supplied, uncomment the placeholder block, replace it with
`![alt text](screenshots/<file>)` at the same point, and set the status here to `SUPPLIED`.
