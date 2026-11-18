# Limitations

Short, specific and deliberate. These are engineering facts, not apologies.

## Team and process

- **Solo developer.** I designed, built, deployed and operate the system. There is no code-review
  process, no second pair of eyes and no handover documentation beyond the code itself.

## Security and operations

- **Admin authentication is a shared secret.** `X-Admin-Key` is compared inline in the quote and
  price-list handlers. There is no identity, role model, audit trail or key rotation.
- **The admin key lives in browser storage.** A stolen browser profile exposes the key. There is
  no server-side session.
- **No observability stack.** Failures are surfaced through application logs and HTTP errors. No
  Application Insights, OpenTelemetry, metrics dashboard or alerting.
- **Request validation is not enforced.** The validation behaviour is registered, but the
  validator scan targets the shell assembly; the product's validator is never resolved.
- **No infrastructure as code.** Deployment is a workflow plus App Service configuration outside
  the repository. There is no Bicep or Terraform.
- **Deployment authenticates with a credential secret,** not OIDC or a federated identity, even
  though the workflow declares `id-token: write`.
- **No rate limiting** on public endpoints beyond reCAPTCHA on quote creation.
- **No automatic quote-notification email.** The backend's Resend registration and send calls are
  disabled. The marketing contact form has its own email route and is separate from quote
  delivery.

## Testing

- **Tests are not in CI.** The suite lives on `test/autopreventivo-xunit`, is not merged, and the
  deploy workflow has no test stage.
- **No integration or HTTP-level tests.** No database, PDF, blob or endpoint coverage.
- **No load or volume testing.** The system has not been exercised at scale.
- **No frontend tests.**

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
