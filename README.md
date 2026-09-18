# Hotel & Property Management SaaS (Hotel PMS)

A multi-tenant Hotel & Property Management SaaS platform. Backend is ASP.NET Core Web API (Clean
Architecture) + EF Core + PostgreSQL; frontend is React + TypeScript + Vite.

## Status: Phase 1 complete

Phase 1 (of the 8-phase build plan) is implemented and verified end-to-end:

- Project scaffolding (Clean Architecture: Domain / Application / Infrastructure / API)
- PostgreSQL schema (`database/schema.sql`) + EF Core migrations layered on top
- JWT authentication (login, register, refresh, logout, forgot/reset/change password, profile)
- Organizations (multi-tenant) and Properties (multi-property per organization)
- Users, Roles (11 seeded roles) and permission-based authorization (26 seeded permissions)
- Tenant isolation enforced in the database layer (global EF query filters), not just the UI
- Audit logging (automatic, via a `SaveChangesAsync` interceptor) for org/property/user changes
- Swagger UI, health checks, Serilog structured logging, rate limiting on auth endpoints
- A working React UI: login, dashboard, and CRUD screens for organizations/properties/users

Rooms, reservations, payments, housekeeping, maintenance, reports, notifications and the SaaS
subscription/billing model are **not yet built** — they land in later phases (see spec section 46).

## Project structure

```
hotel-pms/
├── backend/
│   ├── src/
│   │   ├── PMS.Domain          entities, enums, constants — no dependencies
│   │   ├── PMS.Application     DTOs, services, FluentValidation, interfaces
│   │   ├── PMS.Infrastructure  EF Core, JWT, password hashing, DI wiring
│   │   └── PMS.API             controllers, middleware, Program.cs, Swagger
│   └── tests/PMS.Tests         xUnit unit + WebApplicationFactory integration tests
├── frontend/                   Vite + React + TypeScript
├── database/schema.sql         canonical PostgreSQL schema (source of truth)
├── docker-compose.yml
└── .env.example
```

## Prerequisites

- .NET SDK 10 (LTS)
- Node.js 20+
- Docker (for local PostgreSQL, or point at your own Postgres instance)

## Quick start

1. Copy the environment template and fill in real values (never commit `.env`):

   ```bash
   cp .env.example .env
   ```

2. Start PostgreSQL (skip this if you already have a Postgres instance to use — update
   `ConnectionStrings__DefaultConnection` accordingly):

   ```bash
   docker compose up -d postgres
   ```

3. Apply database migrations. The `InitialBaseline` migration is a no-op for every table already
   created by `database/schema.sql`; it only adds the one table EF's model needs on top
   (`password_reset_tokens`), so it's safe to run against a fresh **or** already-seeded database:

   ```bash
   cd backend
   dotnet tool restore 2>/dev/null || dotnet tool install --global dotnet-ef
   ConnectionStrings__DefaultConnection="<your connection string>" \
     dotnet ef database update --project src/PMS.Infrastructure
   ```

4. Run the API (Development environment seeds roles/permissions mappings plus a demo
   organization, 3 properties and 5 users on first run — see `PMS.Infrastructure/Persistence/Seed/DataSeeder.cs`):

   ```bash
   ASPNETCORE_ENVIRONMENT=Development \
   ConnectionStrings__DefaultConnection="<your connection string>" \
   SeedData__DefaultPassword="ChangeMe123!" \
     dotnet run --project src/PMS.API --urls http://localhost:5080
   ```

   Swagger UI: http://localhost:5080/swagger · Health: http://localhost:5080/health

5. Run the frontend:

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

   http://localhost:5173 — CORS is pre-configured for this origin in `appsettings.Development.json`.

### Seeded demo logins (password: `ChangeMe123!`, or whatever you set `SeedData__DefaultPassword` to)

| Email | Role |
|---|---|
| admin@demo.com | PlatformAdmin (sees every organization) |
| manager@demo.com | OrganizationAdmin (Demo Hospitality Group) |
| frontdesk@demo.com | FrontDesk |
| housekeeping@demo.com | Housekeeper |
| accountant@demo.com | Accountant |

### Docker Compose (full stack)

```bash
docker compose up -d --build
```

Runs Postgres, the API (port `API_HTTP_PORT`, default 5080) and the built frontend behind nginx
(port `FRONTEND_PORT`, default 5173).

## Testing

```bash
cd backend
ConnectionStrings__DefaultConnection="<your connection string>" \
SeedData__DefaultPassword="ChangeMe123!" \
  dotnet test
```

16 tests: JWT claim generation, password hashing, permission-authorization handler (unit), plus
integration tests that hit the real HTTP pipeline for login, tenant isolation and permission
enforcement.

## Configuration

Every secret is sourced from configuration/environment variables — nothing is hardcoded. See
`.env.example` for the full list (`ConnectionStrings__DefaultConnection`, `Jwt__Secret`,
`SeedData__DefaultPassword`, etc.). `appsettings.Development.json` only ever contains dev-only
placeholder values, never real credentials.

## More docs

- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — Clean Architecture layering, tenant isolation,
  the permission model, and why the database schema is SQL-authored rather than EF-first.

DATABASE.md / API.md / DEPLOYMENT.md / DEVELOPMENT.md land alongside the phases that need them
(Docker/K8s specifics in particular belong to Phase 8).
