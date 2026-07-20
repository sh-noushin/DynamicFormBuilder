# Contributing to FormBuilder

Thanks for taking the time to contribute. This document covers the
practical bits: how to get the project running, what the CI expects, and
how to structure a pull request that lands quickly.

## Ground rules

- **Open an issue first** for anything larger than a one-line fix or
  typo. It's a small step that saves days of "actually we don't want
  that shape".
- **One concern per pull request.** If you're touching two unrelated
  areas, split them.
- **Tests stay green.** CI runs `dotnet test` and `ng build` on every
  push; a red PR won't be reviewed until it's green.
- **Be nice.** See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Development setup

Prerequisites:

- .NET 10 SDK
- Node 22 + npm
- (optional) Docker + docker-compose

```bash
# 1. Clone
git clone https://github.com/<your-org>/formbuilder.git
cd formbuilder

# 2. Backend
cd server/FormBuilder.API
dotnet run

# 3. Frontend (in a second terminal, from repo root)
cd client
npm install
npm start
```

The API defaults to `http://localhost:5000`, the SPA to
`http://localhost:4200`. The SPA proxies `/api/*` in dev via Angular's
proxy config, so you can hit the app at `http://localhost:4200`.

Docker route:

```bash
docker compose up --build -d
# open http://localhost:8080
```

## Running the tests

```bash
dotnet test server/FormBuilder.slnx
```

80 xUnit tests cover the core services and mappings. Add tests when you
touch a service; leave the tests you didn't need to touch alone.

## Adding a database migration

Migrations auto-apply on API startup via
`DataSeeder.EnsureDatabaseAsync`. To create a new one after editing an
entity or the `DbContext`:

```bash
dotnet ef migrations add YourMigrationName \
  --project server/FormBuilder.Infrastructure \
  --startup-project server/FormBuilder.API
```

Commit the generated `Migrations/` files with the entity change in the
same PR.

## Code style

- **Backend** — follow the existing patterns: repository interface +
  concrete class in `FormBuilder.Infrastructure`, service interface +
  implementation in `FormBuilder.Core`, controller in `FormBuilder.API`.
  Throw domain exceptions from `FormBuilder.Models.Exceptions`; the
  `DomainExceptionHandler` middleware maps them to HTTP statuses.
- **Frontend** — Angular 22 standalone components, `signal()` /
  `computed()` over RxJS subjects where practical, `@if` / `@for` /
  `@switch` control flow over legacy structural directives. Reuse the
  `Client` service in `api-service.ts` for backend calls; hit
  `HttpClient` directly only for endpoints not covered by the generated
  client (e.g. new admin-only ones).
- Comments should explain **why**, not what the code does. Skip
  boilerplate JSDoc unless a public API needs it.

## Pull request checklist

- [ ] Linked to an issue (or explained why one isn't needed)
- [ ] Tests added for new behaviour
- [ ] `dotnet test server/FormBuilder.slnx` passes locally
- [ ] `npx ng build` from `client/` succeeds
- [ ] New backend endpoints have the right `[Authorize]` scheme +
      roles (writes stay Bearer-only; read endpoints can accept ApiKey)
- [ ] User-facing strings on the public form go through the i18n
      dictionary in `public-form-i18n.ts`
- [ ] Screenshots attached if you touched UI

## Reporting bugs

Open a GitHub issue with:

- The version / commit SHA you're on
- Steps to reproduce
- What you expected vs. what happened
- Relevant logs (redact anything sensitive)

## Reporting security issues

**Do not** open a public issue for a vulnerability. See
[SECURITY.md](SECURITY.md) for the responsible-disclosure process.
