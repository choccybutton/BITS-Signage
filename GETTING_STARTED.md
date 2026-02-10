# Getting Started - BITS Digital Signage Platform

## What's Been Done (Phase 0.1)

✅ **Repository & Solution Structure**
- Clean architecture project layout with 6 core projects
- 4 test projects (xUnit)
- .NET 9 with latest compatible libraries
- Full dependency injection setup

✅ **Build & Dependencies**
- `dotnet build` works cleanly (0 errors, 0 warnings)
- Core NuGet packages installed:
  - **Custom lightweight CQRS** — Open-source CQRS pattern (no licensing concerns)
  - **FluentValidation 12.1.1** — Data validation
  - **EF Core 9.0.2** — ORM with PostgreSQL
  - **Npgsql 9.0.1** — PostgreSQL driver
  - **StackExchange.Redis 2.10.14** — Caching
  - **JWT Bearer 9.0.2** — Authentication
  - **Microsoft.Extensions.DependencyInjection** — Dependency injection

✅ **Docker Local Development**
- `docker-compose.yml` with PostgreSQL 16, Redis 7, MinIO
- All services include health checks
- Run: `cd docker && docker-compose up -d`

✅ **Documentation**
- `ARCHITECTURE.md` — Design patterns, layer responsibilities, tech stack
- `Spec/implementation-plan.md` — 12 phases, ~60 tasks with detailed descriptions

## Next Steps (Phase 0.2 - 0.5)

The following tasks are outlined in the implementation plan and ready to work on:

### **0.2 Database Schema — Core & Auth Tables**
- Create EF Core migrations for Tenant, Venue, User, Device, DevicePairing
- Fluent API configuration for relationships and constraints
- Apply migrations to PostgreSQL

### **0.3 Database Schema — Content & Playlists**
- Migrations for Asset, ContentGroup, Playlist, Schedule tables
- CHECK constraints for assetId XOR contentGroupId
- Indexes on hot query paths (tenantId, venueId, deviceId)

### **0.4 API Foundation — Middleware & Cross-Cutting Concerns**
- Tenant isolation middleware (extract from JWT)
- Authentication & authorization middleware
- Error handling (problem+json format)
- ETag / If-Match validation
- Rate limiting (Redis-backed)
- Pagination & filtering helpers

### **0.5 Storage Service Integration**
- Storage client interface for pre-signed URLs
- CDN URL builder
- Integration with MinIO for local dev

## Quick Start Commands

### Build
```bash
cd src
dotnet build
```

### Start Local Services
```bash
cd docker
docker-compose up -d
# Services running: PostgreSQL (5432), Redis (6379), MinIO (9000/9001)
```

### Run API (future)
```bash
cd src
dotnet run --project BITS.Signage.Api
# Health check: GET https://localhost:7000/health
```

### Run Tests (future)
```bash
cd src
dotnet test
```

### Apply Database Migrations (future)
```bash
cd src/BITS.Signage.Api
dotnet ef database update
```

## Project Structure Quick Reference

```
src/
├── BITS.Signage.Domain/           ← Entities, business logic (no deps)
├── BITS.Signage.Application/      ← Use cases, validators (MediatR)
├── BITS.Signage.Contracts/        ← DTOs, enums (no deps)
├── BITS.Signage.Infrastructure/   ← EF Core, Redis, storage (no ASP.NET refs)
├── BITS.Signage.Api/              ← Controllers, middleware
├── BITS.Signage.Workers/          ← Background jobs
└── Tests/                          ← xUnit test projects

docker/
└── docker-compose.yml             ← PostgreSQL, Redis, MinIO

db/
└── migrations/                    ← EF Core migration files
```

## Key Architectural Decisions

1. **Clean Architecture** — Strict layer separation, testable, framework-agnostic
2. **CQRS** — Commands for writes, Queries for reads (custom open-source implementation, no licensing)
3. **Repository Pattern** — Data access abstraction, easy to mock
4. **Dependency Injection** — All services registered in Program.cs
5. **PostgreSQL + EF Core** — Type-safe LINQ queries, migrations
6. **Redis** — Manifest caching, presence indicators, rate limiting
7. **Modular Monolith** — Clear boundaries, can extract to microservices later

## Common Issues & Solutions

### "dotnet: command not found"
→ Ensure .NET 9 SDK is installed: `dotnet --version` should show `9.0.200+`

### Docker Postgres won't start
→ Ensure port 5432 is not in use: `netstat -an | grep 5432`

### Connection to Redis/Postgres in Code
→ Connection strings will be in `appsettings.json` (after Phase 0.4)
→ Use DI to inject `IConnectionMultiplexer` or `DbContext`

## What's NOT Done Yet

❌ EF Core migrations (Phase 0.2–0.3)
❌ API middleware (Phase 0.4)
❌ Storage service (Phase 0.5)
❌ Authentication endpoints (Phase 1.1)
❌ Database operations
❌ UI (mobile or web)
❌ Player app

## Running Just the Infrastructure

To start PostgreSQL, Redis, and MinIO without the API:

```bash
cd docker
docker-compose up -d postgres redis minio

# Access MinIO console at http://localhost:9001 (admin/admin)
# Connect to Postgres: psql -h localhost -U bits_user -d bits_signage
# Redis CLI: redis-cli
```

## Testing the Build

```bash
cd src
dotnet build                    # Compiles all projects
dotnet test --no-build         # Runs all test projects (currently empty)
```

Expected: All projects compile, tests pass (no tests yet).

## Next Phase Tasks

Once you're comfortable with the structure, the next deliverables are:

1. **Database Schema Migrations** (Phase 0.2–0.3)
2. **API Middleware** (Phase 0.4)
3. **Storage Integration** (Phase 0.5)
4. **Authentication Endpoints** (Phase 1)

Each will be broken into small, testable chunks following the implementation plan.

---

**Questions?** Check `ARCHITECTURE.md` or `Spec/implementation-plan.md` for details.

**Ready to start Phase 0.2?** Begin with database schema migrations using EF Core Fluent API.
