# FormBuilder

A self-hosted, open-source form builder for teams that want the polish of
Typeform / JotForm without the per-seat pricing. Drag together a form in
the admin dashboard, share a link (or QR, or embed iframe), and collect
responses with full ownership of the data.

Ships with **~40 production-ready features** — password gates, response
limits, save-and-resume drafts, webhooks with HMAC signing, per-form
localization, analytics with per-field breakdowns, API keys, JSON
import/export, and more. See the [full feature list](#features) below.

Runs on a single `docker compose up` — everything (API, SPA, SQLite,
uploaded files) is one container stack. Swap SQLite for Postgres by
uncommenting one block in `docker-compose.yml`.

<!--
Add screenshots / GIFs here once you have them:
![Admin dashboard](docs/screenshots/admin-dashboard.png)
![Public form](docs/screenshots/public-form.png)
![Analytics](docs/screenshots/analytics.png)
-->

## Quick start

```bash
docker compose up --build -d
```

Open `http://localhost:8080`. The stack seeds five demo forms (Contact,
Survey, Event Registration, NPS, Job Application) with sample submissions
so the analytics + tags + admin-notes UIs have data on first login.

For production hardening (JWT secret, CORS origins, SMTP), see
[DEPLOYMENT.md](DEPLOYMENT.md).

## Features

### Form building
- Rich field types: Text, Email, Number, Textarea, Phone, Password,
  Date, DateTime, Radio, Select, Checkbox, File, Signature, Rating,
  PageBreak, HiddenField
- Multi-page forms via PageBreak markers
- Show-if conditional visibility (per-field)
- **Skip logic / branch to page** based on answers
- **Answer piping** — reference earlier answers with `{{field_name}}`
- **Markdown** in labels + help text (bold, italic, code, links)
- **Field library** — one-click prebuilt sets (Contact info, Address,
  NPS, Feedback, Consent)
- **Duplicate field** with auto-suffixed name
- Drag-and-drop field reorder
- Per-field validation: required, regex pattern, min/max length,
  numeric range, custom error messages
- Per-field file upload constraints (max size + allowed extensions)
- Field versioning per form with publish + rollback
- **JSON export / import** for backup and cross-env sync

### Public form UX
- Brand color per form (all inputs, buttons, progress bar recolor)
- Custom thank-you message + optional redirect URL
- **Password gate** with header-based verification
- **Response limits** + auto-close date
- **URL prefill** via `?field=value`
- **Save-and-resume drafts** with 30-day resume link
- **Localization** — English / Spanish / French (extensible)
- Custom favicon per form
- Multi-page progress bar

### Anti-abuse
- Honeypot field + per-IP rate limit
- One-response-per-email
- One-response-per-IP

### Sharing
- Public link with short slug
- **QR code** dialog (SVG + download)
- **Embed iframe** dialog with live preview + copy snippet

### Response management
- Search + date-range filter + tag filter
- **Response tags/labels** with color chips
- **Admin notes** per submission
- **Bulk delete** + **bulk CSV export** of selected rows
- **Response detail view** with prev/next navigation
- **Print-friendly** response detail (opens a clean popup)

### Analytics
- Daily submissions bar chart (30-day window)
- Per-field breakdowns for choice fields (Radio, Select, Checkbox,
  Rating) with % distribution

### Integrations
- **Webhooks** with HMAC-SHA256 signing (`X-Webhook-Signature`) and a
  **Slack-formatted** payload mode
- **API keys** — Admin-issued, read-only external access via
  `X-Api-Key` header
- Submitter confirmation emails (via SMTP)
- Admin notification email on submission
- CSV export (full form or selected rows)

### Admin
- Overview dashboard with tiles (total forms, active, submissions,
  last 7 days, recent list, top forms)
- Role-based auth (Admin / User) with JWT
- Auto-migrating SQLite (or Postgres) database

## Tech stack

**Backend** — .NET 10, ASP.NET Core, Entity Framework Core (SQLite or
Postgres), ASP.NET Identity, JWT auth, AutoMapper, `Microsoft.AspNetCore.RateLimiting`.

**Frontend** — Angular 22, Angular Material 22, TypeScript 6, Angular
Signals, standalone components, `@if` / `@for` control flow.

**Delivery** — Multi-stage Docker builds, nginx serving the SPA and
proxying `/api/*` to the API container.

## Architecture

```
+-----------+          +----------------+
|  Browser  |  --->    |  nginx (web)   |
+-----------+          +--------+-------+
                                |
                    static SPA  |  proxy /api/*
                                v
                       +----------------+
                       |  ASP.NET API   |
                       +--------+-------+
                                |
                                v
                       +----------------+
                       |  SQLite/Postgres |
                       +----------------+
```

**Solution layout**

```
server/
├── FormBuilder.API/           # Controllers, middleware, auth wiring
├── FormBuilder.Core/          # Services, DTOs, interfaces, mapping
├── FormBuilder.Infrastructure/  # EF Core context, repositories, SMTP + webhook senders
└── FormBuilder.Models/        # Entities + domain exceptions

client/
└── src/app/                   # Angular SPA
    ├── core/                  # Auth, guards, generated API client
    ├── dashboards/admin/      # Admin UI (forms, submissions, users, api-keys, analytics)
    ├── public-form/           # Anonymous /f/:slug form runtime
    └── shared/                # Header, dialogs, common components

tests/
└── FormBuilder.Tests/         # xUnit backend tests (80 passing)
```

## Development

**Prerequisites**

- .NET 10 SDK
- Node 22 + npm
- (optional) Docker + docker-compose

**Run locally (without Docker)**

```bash
# Terminal 1 — API
cd server/FormBuilder.API
dotnet run

# Terminal 2 — SPA
cd client
npm install
npm start
```

The API defaults to `http://localhost:5000` and the SPA to
`http://localhost:4200`. The SPA proxies `/api/*` in dev via Angular's
proxy config.

**Run tests**

```bash
dotnet test server/FormBuilder.slnx
```

**Applying migrations**

Migrations auto-apply on API startup via
`DataSeeder.EnsureDatabaseAsync`. To create a new migration:

```bash
dotnet ef migrations add <Name> \
  --project server/FormBuilder.Infrastructure \
  --startup-project server/FormBuilder.API
```

## Configuration

Everything else — SMTP setup, CORS origins, JWT secret rotation, Postgres
swap-out — lives in [DEPLOYMENT.md](DEPLOYMENT.md).

## License

TBD. Add a `LICENSE` file before shipping this publicly.

## Contributing

Contributions welcome. Please:

1. Open an issue first for anything larger than a one-line fix
2. Run `dotnet test server/FormBuilder.slnx` locally — CI blocks
   regressions
3. Keep frontend changes buildable via `npx ng build` from `client/`
