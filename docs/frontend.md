# Frontend

Separate repository, deployed on Vercel at `https://www.edilvos.it`. Next.js App Router with
route groups for the public site, the admin area and the Sanity Studio.

## Stack

| Layer | Choice |
|---|---|
| Framework | Next.js 15 (App Router) |
| UI | React 19, TypeScript 5.9, Tailwind CSS 4 |
| Content | Sanity 4 (marketing pages, services, galleries) |
| Forms | Controlled React state, `react-google-recaptcha-v3` |
| Drag and drop | `@dnd-kit/core`, `@dnd-kit/sortable` |
| Analytics | GA4 through `gtag`, Microsoft Clarity, Vercel Analytics |
| Hosting | Vercel; deployment configuration is not checked into the repository |

## Public surfaces

- **Marketing site:** home, services index and service detail pages, gallery, about, contact,
  legal pages. Content comes from Sanity.
- **Quote form** at `/preventivo`: two input modes — a free-text description and a structured
  dimension form — plus client details. reCAPTCHA v3 runs before submission.
- **Result view:** itemised quote, totals, VAT, PDF download. Lines priced by the model fallback
  are marked in the UI rather than presented as catalogue prices.
- **Thank-you page and contact route:** separate server routes for conversion analytics and email.

No authentication is required for these surfaces.

## Admin-only surfaces

The `/admin` route group is not protected by a server session. The operator enters the shared
admin key, which the browser stores in `localStorage` and sends as `X-Admin-Key` to the API.

- **Dashboard** `/admin`: quote list, drafts and final tabs, search/filter, PDF links.
- **CreaTivo** `/admin/creativo`: the manual quote builder — client details, VAT, notes, sections
  and items, drag reorder, live totals, save draft and publish.
- **Price list** `/admin/prezzario`: catalogue browsing and CSV upload.
- **Settings** `/admin/settings`: key and environment checks.
- **Sanity Studio** `/studio`: content editing, separate from the admin key.

Because the key is the only gate, the admin pages are reachable but their data is not. This is a
known limitation, documented in [limitations.md](limitations.md).

## Server actions

The browser never calls the .NET API directly for quote creation. Three Next.js server actions
sit in `app/actions/`:

- `generate-quote.ts` — validates input, forwards the prompt or structured parameters, and keeps
  the API URL server-side.
- `create-manual-quote.ts` — flattens the builder's nested sections into the API's flat item list,
  chooses POST or PUT, and attaches the admin key.
- `price-list.ts` — catalogue queries for the admin builder.

Snippet: [05-create-manual-quote-action.ts](../snippets/05-create-manual-quote-action.ts).

## Price provenance in the UI

Lines returned by the API can carry a provenance tag inside the description. The quote form strips
the tag before rendering and uses it to show whether the price came from the catalogue or from a
model estimate.

Snippet: [06-provenance-tags.tsx](../snippets/06-provenance-tags.tsx).

## Analytics and consent

GA4, Microsoft Clarity and Vercel Analytics are configured. The consent banner sets Google
Consent Mode defaults to denied and updates them on acceptance. GA4 and the Clarity script are
rendered when their environment IDs are present; consent state is passed to Clarity through its
consent API rather than by delaying the script tag. The measurement IDs and project IDs are
environment variables and are not reproduced here.

## What is not in this repository

No frontend source, no `package.json`, no build configuration. The frontend evidence here is the
live site, the surface description, screenshots when supplied, and two reviewed excerpts.
