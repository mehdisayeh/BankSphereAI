# Identity service

Identity is a .NET 9 bounded context using ASP.NET Core Identity 9.0.23, OpenIddict
7.9.0, SQL Server, CQRS feature folders, and Minimal APIs. It adds no Gateway
implementation. The existing banking routes and six banking schemas are retained.

## Architecture

```text
src/Services/Identity/
  BankSphere.Identity.Domain/
    Sessions/RefreshToken.cs
    Permissions/Permission.cs
    Auditing/SecurityAuditLog.cs
  BankSphere.Identity.Application/
    Abstractions/Services/
    DTOs/
    Features/<UseCase>/<Command-or-Query>.cs
    Features/<UseCase>/<Command-or-Query>Handler.cs
    Features/<UseCase>/<Command-or-Query>Validator.cs
  BankSphere.Identity.Infrastructure/
    Identity/                  # ASP.NET Core Identity framework models
    Persistence/Configurations/
    Persistence/Design/
    Persistence/Migrations/
    Services/                  # Identity stores, OIDC adapter, session/audit/notification services
    DependencyInjection.cs
  BankSphere.Identity.Api/
    Contracts/Requests/
    Endpoints/
    Extensions/
    Program.cs
```

Domain and Application retain the existing no-framework/no-package dependency
convention. ApplicationUser, Role, and UserRole belong to the Infrastructure adapter
because they derive from ASP.NET Core Identity framework models. UserManager and
SignInManager own password hashing, security stamps, confirmations, lockout, and
credential verification; these are not reimplemented in the domain.

ApplicationUser has an independent UUID, display name, optional CustomerId, enabled
state, creation/login/password-change timestamps, and the standard Identity
authentication metadata. CustomerId is an explicit optional reference, with no
Customer navigation, cross-schema FK, or automatic Customer registration. Public
registration cannot set it.

The domain RefreshToken represents session/refresh-family metadata: authorization
ID, user, client, security-stamp snapshot, creation/last-use/absolute expiry, and
revocation reason. OpenIddict owns actual encrypted refresh tokens, authorization
codes, token storage, client validation, issuance, and redemption. Raw refresh tokens
and passwords are not stored in the session or audit models.

The new features are Register, ConfirmEmail, Login, Authorize, ExchangeAuthorizationCode,
RefreshToken, Logout, RevokeSessions, ChangePassword, ForgotPassword, ResetPassword,
GetCurrentUser, and deployment-only GrantRole. Each operation, handler, and validator
has a dedicated file. Stateless validators are static; framework password policy
remains centralized in ASP.NET Core Identity.

## Existing roles and permissions

The existing catalog is preserved exactly:

| Role and existing policy name | Matching permission claim |
| --- | --- |
| banking-reader | banking-reader |
| banking-operator | banking-operator |
| fraud-reviewer | fraud-reviewer |
| credit-officer | credit-officer |
| compliance-officer | compliance-officer |
| simulator | simulator |

There was no separate permission catalog before this change. The corresponding
permissions and role grants now use these same names. Existing endpoint policy names
are unchanged; their policies require the existing role, its matching permission,
and the `banking` scope. Additional `permission:<existing-name>` policies support
permission-only checks without creating alternate roles. Roles are independent;
there is no implicit operator-to-reader hierarchy.

Public registration assigns **no roles**. Client-supplied roles, confirmation flags,
customer links, and permissions are not part of registration commands. The first
registered user receives no administrative exception. Catalog initialization seeds
only the six existing roles, matching permissions, and role grants. It is serialized
by a SQL application lock across replicas and never migrates the database implicitly.

Trusted deployment operators can grant an existing role to an enabled, confirmed
account using the configured service credentials and these configuration keys:

```text
Identity__Provisioning__Email
Identity__Provisioning__Role
```

Then execute:

```powershell
dotnet run --project src/Services/Identity/BankSphere.Identity.Api --no-launch-profile -- --grant-role
```

This command opens no web listener, validates the role against the existing catalog,
revokes existing sessions, and writes a role-grant audit event. It requires the same
deployment configuration as the service. There is no public role-management HTTP
endpoint and no bootstrap password or seeded privileged account.

## Authentication flow and endpoints

This is an authorization-code provider with mandatory **S256 PKCE** and refresh-token
rotation. Password, implicit, and client-credentials grants are not enabled. Client
registration is deployment configuration, not an anonymous API. Configured clients
are trusted first-party clients with exact HTTPS redirect URI allowlists and implicit
consent; third-party consent and account UI are not implemented here.

An account UI should be served on the Identity origin. Cross-origin credentialed
CORS is not enabled by default. The Minimal API login returns an authentication
cookie, not an ad-hoc JSON token pair; access tokens are obtained through the standard
OIDC endpoints below.

| Method | Endpoint | Behavior |
| --- | --- | --- |
| GET | /identity/antiforgery | Sets the secure anti-forgery cookie; returns requestToken and headerName |
| POST | /identity/register | Email, password, displayName; requires X-CSRF-TOKEN; returns 202 with no credentials or roles |
| POST | /identity/confirm-email | Email and emailed token; confirms ownership |
| POST | /identity/login | Email, password, optional authenticatorCode; requires X-CSRF-TOKEN; sets a secure login cookie |
| GET | /connect/authorize | Registered client, redirect_uri, response_type=code, scopes, state, S256 challenge; requires the login cookie |
| POST | /connect/token | Standard form-encoded authorization_code or refresh_token exchange |
| GET | /identity/me | JWT-authenticated caller's profile, optional CustomerId, roles, and permissions |
| POST | /identity/logout | Revokes the JWT's current session and clears the login cookie |
| POST | /identity/revoke-sessions | Optional sessionId; omitted/null revokes all caller sessions and changes the security stamp |
| POST | /identity/change-password | Current/new password; JWT subject identifies the caller; revokes all sessions on success |
| POST | /identity/forgot-password | Generic 202 response; reset token is delivered only through the configured sender |
| POST | /identity/reset-password | Email, emailed token, newPassword; revokes all sessions |
| GET | /identity/sessions/validate | JWT-authenticated session/grant check used by banking APIs; no sensitive response body |

OpenIddict also publishes `/.well-known/openid-configuration` and its advertised JWKS
URI. These are publicly readable over HTTPS. Non-health Identity HTTP requests are
rejected when the effective request scheme is not HTTPS. Explicit interactive prompt
requests return `interaction_required`; they never silently reuse a cookie to satisfy
requested reauthentication/consent/account selection. `max_age` is enforced and the
original authentication time is retained in ID tokens.

Typical client sequence:

1. Read `/identity/antiforgery`, retain cookies, and send the returned request token
   in `X-CSRF-TOKEN` when registering/logging in.
2. Confirm the email using the delivered token, then log in. Confirmations and reset
   tokens are never returned by registration or forgot-password responses.
3. Generate a fresh random PKCE verifier and S256 challenge and a client-controlled
   unpredictable state value. Redirect to `/connect/authorize` with the registered
   redirect URI and requested scopes, including `banking` and optionally `offline_access`.
4. Validate the returned state and exchange the code with its verifier at `/connect/token`.
5. Use the returned access token as `Authorization: Bearer ...` at banking APIs. Rotate
   refresh tokens at the same token endpoint and replace the stored refresh token
   atomically. Clients must serialize refresh attempts.

Refresh scopes may be reduced but cannot be expanded beyond the original grant.
Missing/wrong PKCE verifiers, unknown redirect URIs, token replay, expired/revoked
sessions, wrong issuer/audience/signature/type/algorithm, and missing grants are rejected.

## Security and revocation

- Passwords use ASP.NET Core Identity's salted, versioned PBKDF2-HMAC-SHA512 format,
  configured for 220,000 iterations. Password policy requires 14 characters, five unique
  characters, upper/lower case, a digit, and a non-alphanumeric character. Request
  validators cap passwords at 256 characters.
- Five failed credential attempts trigger a 15-minute lockout. Current-password checks
  also count failures. Login verifies an authenticator code when the account already
  has Identity TOTP enabled; enrollment/recovery UI is outside these APIs.
- Confirmed email and enabled account state are required for login and session use.
  Unknown/locked/disabled login failures use a generic response; credential values
  are redacted from command string representations.
- Login cookies are nonpersistent, HttpOnly, Secure, SameSite=Lax, prefixed `__Host-`,
  and expire after ten minutes without sliding renewal. Registration/login require
  anti-forgery protection. Bearer-protected self-service endpoints derive ownership
  from the validated subject; they accept no caller-selected user ID.
- Access tokens are RS256-signed JWTs with a five-minute lifetime, issuer/audience
  validation, `at+jwt` type validation, and 30-second clock skew. The audience defaults
  to the existing `banksphere` value. Symmetric SigningKey fallback is removed.
- Authorization codes last two minutes. Refresh families have an absolute 30-day
  lifetime. Codes and refresh tokens are protected by a separate encryption certificate;
  access tokens are signed and intentionally readable by resource servers.
- OpenIddict persists token status and rotates refresh tokens. Replay detection also
  revokes the authorization/session and records an audit event. A SQL application lock
  serializes each code/refresh redemption across instances, covering the complete
  validation/issuance pipeline. The lock name is a SHA-256 digest, not the credential.
  Lock connections use a separate bounded pool from EF store connections.
- Every banking API validates signature, algorithm, issuer, audience, type, and expiry
  locally via HTTPS discovery/JWKS, then checks session status with Identity over HTTPS.
  The check includes enabled/confirmed state, lockout, security stamp, authorization
  status, and current role/permission grants. Revocation therefore takes effect on the
  next request without waiting five minutes. Identity outages fail closed. This adds
  one network round trip per authenticated API request; there is no stale-grant cache.
- Audit records persist user ID where known, event, outcome, UTC time, and correlation ID.
  Registration, login, confirmations, password operations, grants, token exchanges,
  replay detection, token failures, and session revocations are audited without storing
  passwords, confirmation/reset tokens, or bearer credentials.
- Credential/token endpoints have a fixed-window per-IP limiter (30 requests/minute by
  default), no queue, bounded form fields, and a 32 KiB Kestrel request-body limit.
  Trusted proxy addresses are explicit; arbitrary forwarded client IPs are not trusted.

## Deployment configuration

No passwords, client secrets, private keys, certificate passwords, or private PFX files
are committed. Supply secrets with the deployment's .NET configuration/secret provider.

| Configuration | Purpose |
| --- | --- |
| ConnectionStrings__Banking | Existing shared SQL Server database; use BankSphereAI_Development locally |
| Authentication__Authority | Same absolute HTTPS root issuer URI on Identity and all banking APIs; default https://localhost:7100/ |
| Authentication__Audience | Existing banksphere audience; retain it consistently |
| ASPNETCORE_URLS | HTTPS Identity binding, e.g. https://localhost:7100 |
| AllowedHosts | Identity hostname allowlist for the deployment |
| Identity__SigningCertificate__Path / Password | External private RSA PFX, at least 3072 bits, currently valid |
| Identity__EncryptionCertificate__Path / Password | Separate external private RSA PFX, at least 3072 bits, currently valid |
| Identity__DataProtection__Directory | Persistent, access-controlled key directory shared by Identity replicas; keys are encrypted with the encryption certificate |
| Identity__Notifications__Host / Port / From | SMTP transport and sender; STARTTLS on the default 587, implicit TLS on 465 |
| Identity__Notifications__Username / Password | SMTP credentials from the secret provider when authentication is required |
| Identity__Notifications__AccountActionUri | HTTPS account UI URI receiving purpose, email, and encoded confirmation/reset token |
| Identity__Clients__0__ClientId | Registered first-party public OIDC client identifier |
| Identity__Clients__0__RedirectUris__0 | Exact HTTPS callback allowlist entry; repeat indices for additional entries/clients |
| Identity__TrustedProxies__0 | Optional trusted reverse proxy IP; repeat indices as needed |
| Identity__RateLimit__PermitLimit | Optional per-instance/IP requests per minute, 1..300; default 30 |

Production startup requires external signing/encryption certificates, a persistent key
directory, and notification settings. Separate signing/encryption keys are enforced.
Development alone permits OpenIddict-generated development certificates, persisted outside
the repository. Those token credentials are separate from the HTTPS hosting certificate;
configure/trust the local HTTPS certificate for browser/API use. Never disable production
metadata or SMTP TLS certificate validation.

Run schema updates explicitly before service startup:

```powershell
dotnet tool restore
dotnet ef database update --context IdentityDbContext --project src/Services/Identity/BankSphere.Identity.Infrastructure
dotnet run --project src/Services/Identity/BankSphere.Identity.Api --no-launch-profile
```

The design-time factory retains the same BANKSPHERE_SQL_CONNECTION override and local
SQL Express default as the banking factories. Runtime uses ConnectionStrings:Banking.
AppHost includes Identity on HTTPS port 7100 but still requires external configuration;
AppHost/Gateway startup was not used for verification.

Configure deployment monitoring, backups, audit retention/export, token-store maintenance,
certificate rotation with an overlap plan, and distributed edge rate limiting for multiple
replicas. The built-in limiter is per process. No account UI, SMTP server, external secret
manager, TLS termination infrastructure, or Gateway was built as part of this service.

## Database and verification

The Identity migration is applied to the existing BankSphereAI_Development database.
All tables, indexes, and FKs are inside `identity`; its history is
`identity.__EFMigrationsHistory`. Existing banking tables and data were not recreated.

Tables: Users, Roles, UserRoles, AspNetUserClaims, AspNetRoleClaims, AspNetUserLogins,
AspNetUserTokens, Permissions, RolePermissions, RefreshTokens, SecurityAuditLogs,
OpenIddictApplications, OpenIddictAuthorizations, OpenIddictScopes, OpenIddictTokens,
and __EFMigrationsHistory. The model has 32 indexes/keys and 12 FKs, excluding the
history-table primary key. Catalog initialization provisioned six roles, six permissions,
and six grants in development; no users or credentials were seeded.

Unit tests cover session invariants, input validation/redaction, all six role/permission
policies, and asymmetric JWT validation. SQL-backed integration tests exercise the actual
HTTPS request pipelines via TestServer, Identity/OpenIddict stores, and all six real banking
endpoint registrations. They verify successful banking reads, anonymous rejection,
insufficient-grant rejection, tampering, confirmation/lockout/CSRF, password changes/resets,
ownership, logout/revoke-all, refresh rotation/replay/concurrency, code replay/PKCE,
scope reduction, fail-closed outages, immediate permission removal, rate limiting, HTTPS,
and rejection of production development-key fallback.

Test certificates/passwords are randomly generated, notifications are captured in memory,
and SQL uses a uniquely named BankSphereAI_IdentityTests_* database with guarded cleanup.
No email is sent to real recipients. Production SMTP delivery, external account UI, and
real TLS/proxy deployment remain deployment verification steps.

Executed build/test/migration results are recorded at the top of
[verification.md](verification.md).

Implementation references: [OpenIddict server integration](https://documentation.openiddict.com/guides/getting-started/creating-your-own-server-instance),
[OpenIddict token storage and revocation](https://documentation.openiddict.com/configuration/token-storage),
[signing/encryption credentials](https://documentation.openiddict.com/configuration/encryption-and-signing-credentials),
[ASP.NET Core Identity configuration](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-9.0),
and [OWASP PBKDF2 work factors](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#pbkdf2).
