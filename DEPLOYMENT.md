# Deployment

The repository ships a two-container docker-compose stack that gets the
whole app running with a single command. SQLite is the default database so
there is no external dependency; a Postgres block is included but commented
out in `docker-compose.yml`.

## Quick start

```
docker compose up --build -d
```

Open `http://localhost:8080`. Log in with the seeded admin account (see
`SeedOptions` in `appsettings.Development.json`; the seeder only runs in
Development mode by design — for a Production image you need to create the
first admin yourself, either via the seeded appsettings or by inserting a
row directly).

## Services

| Service | Image built from | Host port | Notes |
|---|---|---|---|
| `api` | `server/FormBuilder.API/Dockerfile` | none (internal only) | ASP.NET 10 Kestrel on `:8080` inside the network. Volume mounts `./data/api` → `/app/data` for SQLite + uploads. |
| `web` | `client/Dockerfile` | `8080:80` | nginx serving the Angular SPA and proxying `/api/*` to the api container. |

## Persistent data

Everything the API writes lives under `./data/api` on the host:

```
./data/api/
├── FormBuilder.db          # SQLite database (forms, submissions, drafts, ...)
├── FormBuilder.db-shm      # SQLite shared memory
├── FormBuilder.db-wal      # SQLite write-ahead log
└── uploads/                # Submitted files (per FileUploads:StoragePath)
```

Back up the whole directory to back up the app. Restore by dropping it into
place before starting the stack.

## Configuration you must change before production

`docker-compose.yml` uses safe defaults for local use. Before shipping to a
public host, set the following via a `.env` file (docker-compose picks it up
automatically) or your orchestrator's secret store:

- **`Jwt__Key`** — any random string ≥ 32 characters. Rotating this
  invalidates every existing JWT, so pick once and keep it.
- **`Cors__AllowedOrigins__0`** — the exact origin your users hit
  (e.g. `https://forms.yourdomain.com`). Add more indexed entries
  (`__1`, `__2`, …) if you embed forms on multiple domains.
- **`Notifications__SmtpHost` / `SmtpPort` / `SmtpUsername` /
  `SmtpPassword` / `AdminEmail`** — required if you want admin
  submission notifications or submitter confirmation emails to actually
  send. Without these the notifiers log-and-skip.

## Healthcheck

The API exposes `GET /healthz` (unauthenticated, returns
`{"status":"ok"}`). Compose polls it every 30s to gate the `web` service
on the API being up. Deliberately does not touch the database — a stalled
DB should surface via slow `/api/forms` responses, not by restarting the
container.

## Applying database migrations

All pending EF Core migrations run automatically on API startup via
`DataSeeder.EnsureDatabaseAsync`. If you're upgrading from an existing
volume, no manual step is needed — start the new image and the schema
catches up. Snapshot the `data/` directory before you do this if the DB
holds anything you can't afford to lose.

## Swapping SQLite for Postgres

Uncomment the `db` service block in `docker-compose.yml`, then change the
API service's `ConnectionStrings__DefaultConnection` env var to a Npgsql
connection string like:

```
Host=db;Database=formbuilder;Username=formbuilder;Password=change-me
```

You'll also need to add `Microsoft.EntityFrameworkCore.PostgreSQL` to
`FormBuilder.Infrastructure.csproj` and swap the `UseSqlite(...)` call in
`ServiceCollectionExtensions.AddFormBuilderDbContext` for `UseNpgsql(...)`.
The rest of the code is provider-agnostic.

## Local rebuild after code changes

```
docker compose build            # rebuild whichever image needs it
docker compose up -d             # rolling replace
docker compose logs -f api web  # tail logs
```
