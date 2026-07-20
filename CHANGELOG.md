# Changelog

All notable changes to FormBuilder are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] — 2026-07-20

First tagged release. Everything below is new relative to the initial
scaffolding.

### Added — Form building
- Rich field library: Text, Email, Number, Textarea, Phone, Password,
  Date, DateTime, Radio, Select, Checkbox, File, Signature, Rating,
  PageBreak, and HiddenField
- Multi-page forms via PageBreak markers with progress bar
- Show-if conditional field visibility with a JSON rule schema
- Skip-logic on PageBreak fields — jump to a specific page based on an
  answer
- Answer piping — `{{field_name}}` tokens in labels / help text /
  placeholders re-render live as the visitor types
- Markdown subset in labels + help text: `**bold**`, `*italic*`,
  `` `code` ``, and `[text](url)` with URL restricted to http(s)/mailto
- Field library / templates — one-click curated sets (Contact info,
  Address, NPS, Feedback rating, Consent) with name-collision
  auto-suffixing
- Duplicate field action that clones every property and inserts the
  copy right after its source
- Drag-and-drop field reordering
- Per-field validation UI: regex pattern, min/max length, numeric
  range, all with custom error messages
- Per-field file constraints: max size in MB + allowed extension list
- Field versioning per form with publish + set-current + rollback
- JSON export / import of form definitions (backup + cross-env sync)

### Added — Public form UX
- Per-form brand color that recolors every input / button / progress
  bar via `--mat-sys-primary` override
- Custom thank-you message and optional redirect URL after submit
- Password gate with header-based verification (`X-Form-Password`)
  running before every read + write
- Response limits (max submissions) + auto-close date with a friendly
  closed state
- URL prefill via `?field_name=value` — pipes into every field type
  with sensible per-type coercion (Checkbox accepts truthy strings;
  Radio/Select only accept option values)
- Save-and-resume drafts with a 30-day resume link
- Localization for built-in UI strings (English / Spanish / French)
- Custom favicon per form on the public `/f/:slug` page
- Print-friendly response detail via a clean popup

### Added — Anti-abuse
- Off-screen honeypot field (position: absolute; beats display:none)
  with server-side 400 on any non-empty value
- Per-IP fixed-window rate limit on the public submit endpoint
  (10 req/min via `Microsoft.AspNetCore.RateLimiting`)
- One-response-per-email (case-insensitive across all versions)
- One-response-per-IP (companion for forms that don't collect email)

### Added — Sharing + distribution
- Copy-to-clipboard public share link
- QR code dialog with SVG generation + download
- Embed iframe dialog with size controls, live preview, and copy
  snippet

### Added — Response management
- Admin submissions view with search, date-range filter, and tag
  filter (all compose as AND)
- Bulk row selection with tri-state header checkbox
- Bulk delete + bulk CSV export of selected rows
- Response detail dialog with prev/next navigation across the
  currently-filtered slice
- Per-submission admin notes (private, admin-only) and tags with
  auto-normalize (trim / lower / dedupe / cap 20 tags of 40 chars)
- CSV export (full form or selected rows)

### Added — Analytics
- Daily submissions bar chart (last 30 days, adjustable via query
  param, clamped to [1, 365])
- Per-field breakdowns for choice-type fields (Radio, Select,
  Checkbox, Rating) with % distribution

### Added — Integrations
- Webhooks with HMAC-SHA256 signing (`X-Webhook-Signature: sha256=hex`)
  and per-form signing secrets
- Slack-formatted webhook payloads (per-form toggle) for direct
  Slack channel wiring
- Admin-issued API keys (`fbk_<32 hex>`) with SHA-256-hashed storage
  and a display prefix; usable via `X-Api-Key` header on read-only
  endpoints
- Submitter confirmation emails via SMTP with custom subject + body
- Admin submission notification emails

### Added — Admin
- Overview dashboard with tiles (total forms, active, submissions
  total + last 7 days, recent 5, top 3 by count)
- Role-based auth (Admin / User) with JWT
- API key management page (mint / list / revoke; raw key surfaced
  exactly once at mint time)

### Added — Infrastructure
- Multi-stage Dockerfile for the API (.NET 10 SDK → aspnet:10.0)
- Multi-stage Dockerfile for the SPA (node:22 → nginx:1.27)
- `docker-compose.yml` two-service stack (api + web) with volumes for
  SQLite + uploads and a commented Postgres 16 block
- `/healthz` unauthenticated liveness endpoint gating web-service
  startup on api readiness
- GitHub Actions CI workflow: independent backend (dotnet test) and
  frontend (ng build) jobs with package caching
- Enriched demo seed data (5 forms in distinct brand colors + 8 seeded
  NPS submissions across 14 days) so first login has populated
  analytics + tag chips + admin notes

### Security
- HMAC-signed webhook delivery
- SHA-256 hashed API keys (raw key never persisted)
- Constant-time comparison on form-level access passwords
- Redirect + Webhook URLs restricted to http(s) schemes to block
  `javascript:` / `data:` injection
- File upload constraints enforced at both system-wide and per-field
  level
- Escape-then-markdown pipeline for label rendering; raw HTML in
  labels or piped values can never reach the DOM as markup

### Documentation
- Product-facing README with feature list, tech stack, architecture,
  and dev + docker quick-starts
- `DEPLOYMENT.md` covering production config, healthcheck story, and
  Postgres swap-out recipe
- `CONTRIBUTING.md` with dev setup, CI expectations, and PR checklist
- `CODE_OF_CONDUCT.md` (Contributor Covenant 2.1)
- `SECURITY.md` with responsible-disclosure process + scope
- MIT license

### Database migrations applied in this release
1. `InitialCreate`
2. `fix`
3. `AddFormSlug`
4. `AddShowIfCondition`
5. `AddBrandColor`
6. `AddFormAccessPassword`
7. `AddFormThankYouAndRedirect`
8. `AddFormResponseLimits`
9. `AddFormWebhook`
10. `AddFormOneResponsePerEmail`
11. `AddFormConfirmationEmail`
12. `AddSubmissionAdminNotes`
13. `AddSubmissionTags`
14. `AddFormSubmissionDraft`
15. `AddApiKey`
16. `AddFormLocale`
17. `AddFormWebhookSlackFaviconIp`

All auto-apply on startup via `DataSeeder.EnsureDatabaseAsync`, so
upgrading from an older checkout only requires pulling + restarting.

[1.0.0]: https://github.com/nooshinsh/formbuilder/releases/tag/v1.0.0
