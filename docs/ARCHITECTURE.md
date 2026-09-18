# Architecture

## Layering (Clean Architecture)

```
PMS.API            controllers, middleware, auth policies, Program.cs composition root
   ↓ depends on
PMS.Infrastructure  EF Core DbContext, repositories, JWT, password hashing, Serilog
   ↓ depends on
PMS.Application     DTOs, FluentValidation validators, service interfaces + implementations
   ↓ depends on
PMS.Domain          entities, enums, constants — no dependencies on anything else
```

Dependencies only point inward. `PMS.Application` defines `IUnitOfWork` / `IRepository<T>` /
`ICurrentUserService` / `ITokenService` / `IPasswordHasher` as interfaces; `PMS.Infrastructure`
provides the EF Core / JWT / ASP.NET Identity-hasher implementations. Controllers never touch EF
Core or the database directly — they call an Application service and translate the result into an
`ApiResponse<T>`.

## Why the database schema is SQL-authored, not EF-first

`database/schema.sql` is the canonical schema, written directly in SQL (snake_case tables/columns,
explicit constraints, a couple of Postgres functions/views for future phases) and applied via
Postgres's `docker-entrypoint-initdb.d` mechanism. EF Core's entity configurations
(`PMS.Infrastructure/Persistence/Configurations/*`) map onto that exact schema via Fluent API
(`ToTable`, `HasColumnName`, etc.) rather than generating it.

Consequently, custom User/Role/Permission entities are used instead of the standard ASP.NET
Identity table set (`AspNetUsers`, `AspNetRoles`, …) — the schema's `users`/`roles`/`permissions`
tables are much simpler than Identity's, so password hashing uses Identity's `PasswordHasher<T>`
standalone (not the full `UserManager`/`SignInManager`/`IdentityDbContext` stack).

The first EF Core migration (`InitialBaseline`) is a **baseline migration**: every table it would
normally create already exists from `schema.sql`, so its `Up()` was hand-trimmed to only create the
one genuinely additive table (`password_reset_tokens`, needed for the forgot/reset-password flow
that the original schema didn't include). Running `dotnet ef database update` against either a
fresh database (schema.sql applied) or one that already has the full schema behaves identically —
it just records `InitialBaseline` as applied and adds `password_reset_tokens`. Future schema
changes (rooms, reservations, payments, …) get their own ordinary migrations from that point.

## Tenant isolation

Every tenant-scoped entity (`Organization`, `Property`, `User`) has an EF Core global query filter
registered in `ApplicationDbContext.OnModelCreating`:

```csharp
modelBuilder.Entity<Property>().HasQueryFilter(p =>
    currentUser.IsPlatformAdmin || p.OrganizationId == currentUser.OrganizationId);
```

`ICurrentUserService` reads the caller's `org_id` claim and role claims from the current
`HttpContext`. This means tenant isolation is enforced **at the query root**, independent of
whether a given service method remembers to filter — the spec's rule #11/#12 ("users must never
access another organization's data") is a database-layer guarantee, not just an application
convention. Two things intentionally bypass it:

- `IRepository<T>.QueryUnfiltered()` — used only by pre-authentication flows (login, registration,
  password reset, refresh-token lookup) that must find a single row belonging to *some* tenant
  before a tenant context exists yet.
- The `DesignTimeDbContextFactory` used by `dotnet ef` commands, which never touches real data.

## Permission model

JWT access tokens carry the caller's roles (`role` claims) and their union of permissions
(`permission` claims) at issue time — looked up once at login/refresh, not on every request. A
custom `[HasPermission("properties.create")]` attribute resolves to a dynamically-built
authorization policy (`PermissionPolicyProvider`) backed by `PermissionAuthorizationHandler`, which
succeeds if the caller is `PlatformAdmin` or holds the exact permission claim. Roles and their
permission sets are seeded by `DataSeeder` (Development only) — `database/schema.sql` seeds the
`roles` and `permissions` reference tables themselves, but not the mapping between them, since
that's an application concern rather than a fixed schema fact.

## Audit logging

`ApplicationDbContext.SaveChangesAsync` inspects the change tracker before saving and writes an
`AuditLog` row for every Added/Modified/Deleted `Organization`/`Property`/`User` entity in the same
transaction — old/new values as JSON, keyed by the current user/organization. No service method has
to remember to call an audit API.
