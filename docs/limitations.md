# Limitations

Short, specific and deliberate. These are engineering facts, not apologies.

## Team and process

- **Solo developer.** I designed, built, deployed and operate the system. There is no code-review
  process, no second pair of eyes and no handover documentation beyond the code itself.

## Security and operations

- **Admin authentication is a shared secret.** `X-Admin-Key` is now checked by one fail-closed,
  constant-time endpoint filter across all 11 admin routes. There is still no identity, role
  model, per-user audit trail, key rotation or expiry.
- **The admin key lives in browser storage.** A stolen browser profile exposes the key. There is
  no server-side session.
- **No per-user attribution.** Every admin action has the same identity: whoever holds the key.
- **Observability is collection, not alerting.** Serilog, Application Insights, correlation IDs
  and `/health` are live. There are no alert rules, dashboards or dependency-health checks.
- **Validation is partial.** `POST /api/quotes/manual` is validated. `PUT /api/quotes/{id}` uses
  the same DTO but has no validator.
- **No infrastructure as code.** Deployment is a workflow plus App Service configuration outside
  the repository. There is no Bicep or Terraform.
- **Deployment authenticates with a credential secret,** not OIDC or a federated identity, even
  though the workflow declares `id-token: write`.
- **No rate limiting** on public endpoints beyond reCAPTCHA on quote creation.
- **No automatic quote-notification email.** The backend's Resend registration and send calls are
  disabled. The marketing contact form has its own email route and is separate from quote
  delivery.
- **The App Service plan is F1 Free with no deployment slot.** Repeated crashes can exhaust the
  worker-stop quota and disable the site. A risky runtime change has no slot-based rollback.

## Testing

- **Tests are in CI but in-process only.** The 48 tests use mocks and a `WebApplicationFactory`
  TestServer. They do not use a real SQL Server, Azure SQL, Blob Storage, Qdrant or OpenAI
  endpoint.
- **No integration or end-to-end tests.** No coverage of a real database, real blob storage, PDF
  rendering on a real file system or the deployed environment.
- **No load or volume testing.** The system has not been exercised at scale.
- **No frontend tests are in this case study.**

## AI and data

- **Qdrant was a local prototype.** The semantic candidate search is wired in code but no vector
  store is provisioned by the deployment. Treat vector-search behaviour as local-development
  evidence, not a production claim.
- **The model-estimate fallback is not separately evaluated.** It exists, it is labelled, and it
  was outside the thesis measurement.
- **The model provider is OpenAI API at this snapshot.** Moving to Azure OpenAI is planned, not
  done.
- **No pricing provenance is persisted.** A quote stores the final unit price, not which pipeline
  stage produced it. The `AI_ESTIMATE` code and description tag are the only durable trace.
- **Evaluation has no holdout set.** The thesis used the same 42 estimates for tuning and
  reporting.

## Product

- **No multi-tenant model.** One cooperative, one catalogue, one region.
- **No versioned price lists.** A catalogue change affects future quotes; historical quotes keep
  their stored line values but there is no price-version reference.
- **No role separation** between viewing quotes, editing drafts and publishing final quotes.

These gaps are the reason the next-steps list looks the way it does.
