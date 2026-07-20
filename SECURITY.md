# Security Policy

## Reporting a vulnerability

**Please do not open a public GitHub issue for a security vulnerability.**

If you believe you've found a vulnerability in FormBuilder, email
**security@formbuilder.local** (replace with a real address before
publishing) with:

- A description of the issue and its potential impact
- Steps to reproduce (a proof-of-concept payload if you have one)
- The commit SHA or docker tag you tested against
- Your name/handle for credit in the fix's release notes, if you'd like it

We aim to acknowledge reports within **3 business days** and to ship a
fix (or a clear timeline for one) within **14 days** of acknowledgment.
For high-severity issues we'll coordinate a disclosure date with you.

## Scope

In scope:

- Authentication + authorization flows (JWT, API keys, role checks)
- The public `/api/public/forms/*` surface
- Anti-abuse mechanisms (rate limiting, honeypot, one-response-per-*)
- File upload handling (`/api/uploads`)
- Webhook signing and payload delivery
- CSV export + form JSON import/export paths
- Cross-site scripting on the public form and admin UI

Out of scope (please don't file these as security issues):

- Denial-of-service via legitimately expensive but authorized calls
  (e.g., an admin exporting a huge CSV)
- Missing hardening on `appsettings.Development.json` (dev-only file)
- Issues that require a compromised admin session to exploit (assume
  admins are trusted)
- Third-party dependencies with known CVEs already tracked upstream

## What "responsible disclosure" means here

- **Give us time to fix.** We ask reporters not to publish details until
  a patched release is out.
- **Don't attack production systems.** Test against your own local
  docker-compose instance.
- **Don't access or modify data that isn't yours** while probing.

## Security features already shipped

For context, these are the security-adjacent features already in
FormBuilder — worth checking their configuration before reporting an
issue as a bug:

- **JWT auth with a rotatable signing key** (`Jwt__Key` env var)
- **API keys stored as SHA-256 hashes**; raw key surfaced exactly once
  at mint time
- **HMAC-SHA256 webhook signatures** via `X-Webhook-Signature`
- **Honeypot + per-IP rate limit** on the public submit endpoint
- **Redirect URL + Webhook URL** validation restricted to `http(s)`
  schemes to block `javascript:` / `data:` injection
- **File upload constraints** at both global and per-field level
- **HTML-escape then markdown** in the public-form label rendering
  path so raw tags can never reach the DOM as markup
- **Constant-time password comparison** on form-level access passwords

## Credit

Reporters who follow the responsible-disclosure process above are
credited in the release notes of the patch (or listed anonymously if
they prefer).
