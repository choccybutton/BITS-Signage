# BITS Digital Signage Platform - Clean Architecture Design

## Overview

This project follows **Clean Architecture** principles with a modular monolith structure. The architecture supports multiple clients (mobile app, Fire TV player) and background workers through clearly separated layers.

## Project Structure

```
BITS-Signage/
├── src/
│   ├── BITS.Signage.Domain/           # Domain entities, value objects, interfaces
│   ├── BITS.Signage.Application/      # Use cases, commands, queries, validators
│   ├── BITS.Signage.Contracts/        # DTOs, request/response models, constants
│   ├── BITS.Signage.Infrastructure/   # EF Core, repositories, external services
│   ├── BITS.Signage.Api/              # ASP.NET Core endpoints, middleware
│   └── BITS.Signage.Workers/          # Background job host, scheduled tasks
├── src/Tests/
│   ├── BITS.Signage.Domain.Tests/
│   ├── BITS.Signage.Application.Tests/
│   ├── BITS.Signage.Infrastructure.Tests/
│   └── BITS.Signage.Integration.Tests/
├── docker/
│   └── docker-compose.yml             # Local dev environment (PostgreSQL, Redis, MinIO)
├── db/
│   └── migrations/                    # EF Core migrations
└── Spec/                              # Platform specification & implementation plan
```

## Architectural Layers

### 1. Domain Layer (`BITS.Signage.Domain`)

**Responsibility:** Define core business logic and domain entities independent of frameworks.

**Contains:**
- Domain entities (Tenant, Venue, Device, Asset, Playlist, ContentGroup, etc.)
- Value objects (AssetId, PlaylistId, PairingCode, etc.)
- Domain interfaces (repository contracts)
- Business rules and validation
- Domain events (future event sourcing support)

**Dependencies:** None (no external dependencies)

### 2. Application Layer (`BITS.Signage.Application`)

**Responsibility:** Implement use cases and application logic using domain entities.

**Contains:**
- **Commands** — Write operations (CreatePlaylist, PublishContentGroup, etc.)
- **Queries** — Read operations (GetPlaylist, ListAssets, etc.)
- **Handlers** — Command and query processing
- **Validators** — Fluent validation rules
- **Mappers** — DTO ↔ Domain entity conversion
- **Interfaces** — Abstractions for infrastructure concerns

**Technology:** MediatR, FluentValidation

**Dependencies:** Domain layer

### 3. Contracts Layer (`BITS.Signage.Contracts`)

**Responsibility:** Define API DTOs, enums, and shared constants.

**Contains:**
- Request DTOs (e.g., `CreatePlaylistRequest`)
- Response DTOs (e.g., `PlaylistResponse`)
- Enums (AssetType, DeviceStatus, ResourceScope, etc.)
- Constants (error codes, pagination defaults)

**Technology:** None (POCOs only)

**Dependencies:** None

### 4. Infrastructure Layer (`BITS.Signage.Infrastructure`)

**Responsibility:** Implement data access and external service integration.

**Contains:**
- **DbContext** — Entity Framework Core configuration
- **Repositories** — Data access abstractions (implement IRepository)
- **Database Migrations** — EF Core migrations for schema
- **Redis Client** — Caching service
- **Storage Service** — Object storage (S3/Azure/MinIO)
- **Background Job Queue** — Job scheduling (Hangfire or similar)
- **Implementations** of application layer interfaces

**Technology:** Entity Framework Core 9, Npgsql, StackExchange.Redis, Hangfire

**Dependencies:** Application, Domain layers

### 5. API Layer (`BITS.Signage.Api`)

**Responsibility:** HTTP endpoints, middleware, dependency injection.

**Contains:**
- **Controllers/Endpoints** — REST API routes
- **Middleware** — Authentication, authorization, error handling, rate limiting
- **Configuration** — Dependency injection setup, logging
- **Health checks** — Service health monitoring
- **Filters** — Cross-cutting concerns (ETag, If-Match validation)

**Technology:** ASP.NET Core 9

**Dependencies:** Application, Infrastructure, Contracts layers

### 6. Workers Layer (`BITS.Signage.Workers`)

**Responsibility:** Background job processing independent of HTTP requests.

**Contains:**
- **Job Classes** — Asset processing, cleanup, content group propagation
- **Job Handlers** — Process background work
- **Scheduling** — Cron expressions, triggers

**Technology:** Hangfire or similar background job framework

**Dependencies:** Application, Infrastructure layers

## Clean Architecture Principles

### Dependency Rule
- **Inner layers never depend on outer layers**
- Entities → Application → Infrastructure → API/Workers
- Outer layers depend inward

### Testability
- Each layer can be unit tested independently
- Domain layer has zero dependencies
- Application layer depends only on domain
- Infrastructure provides test doubles/mocks

### Technology Agnostic
- Domain entities don't reference database attributes
- Application doesn't reference ASP.NET Core
- Easy to swap Redis for in-memory cache
- Easy to switch from PostgreSQL to another DB

## Technology Stack

- **Language:** C# 13
- **Framework:** .NET 9
- **Web API:** ASP.NET Core 9
- **Database:** PostgreSQL 16
- **ORM:** Entity Framework Core 9
- **Caching:** Redis
- **Object Storage:** MinIO (local) / AWS S3 (production)
- **Background Jobs:** Hangfire (TBD)
- **Validation:** FluentValidation
- **CQRS:** MediatR
- **Testing:** xUnit, Moq
- **API Auth:** JWT tokens

## Development Setup

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose
- PostgreSQL 16+ (or use docker-compose)
- Redis (or use docker-compose)

### Quick Start

1. **Start local infrastructure:**
   ```bash
   cd docker
   docker-compose up -d
   ```

2. **Build solution:**
   ```bash
   cd src
   dotnet build
   ```

3. **Apply migrations:**
   ```bash
   cd BITS.Signage.Api
   dotnet ef database update
   ```

4. **Run API:**
   ```bash
   dotnet run --project BITS.Signage.Api
   ```

5. **Run tests:**
   ```bash
   dotnet test
   ```

API will be available at `https://localhost:7000` (or configured port).
Health check: `GET https://localhost:7000/health`

## Project References

```
Contracts ← (no dependencies)

Domain ← (no dependencies)

Application ← Domain, Contracts

Infrastructure ← Application, Domain, Contracts

Api ← Application, Infrastructure, Contracts

Workers ← Application, Infrastructure, Contracts

Domain.Tests ← Domain

Application.Tests ← Application, Domain, Contracts

Infrastructure.Tests ← Infrastructure, Application, Domain, Contracts

Integration.Tests ← Api, Application, Infrastructure, Domain, Contracts
```

## Key Design Patterns

### CQRS (Command Query Responsibility Segregation)
- **Commands** modify state (CreatePlaylist, PublishContentGroup)
- **Queries** retrieve data (GetPlaylist, ListAssets)
- Separated by MediatR handlers

### Repository Pattern
- Abstract data access behind `IRepository<T>`
- Domain doesn't know about databases
- Easy to mock in tests

### Dependency Injection
- All services registered in Program.cs
- Interfaces injected into constructors
- Loose coupling between layers

### Value Objects
- Immutable objects that represent concepts (PlaylistId, Checksum, etc.)
- Encapsulate validation
- Type-safe IDs

## Testing Strategy

### Unit Tests
- **Domain.Tests** — Entity behavior, business rules
- **Application.Tests** — Use case logic, validation
- **Infrastructure.Tests** — Repository implementations (with test DB)

### Integration Tests
- **Integration.Tests** — Full stack with real PostgreSQL/Redis
- Test actual HTTP endpoints
- Verify persistence and caching

### Test Doubles
- Moq for mocking interfaces
- In-memory databases for infrastructure tests
- Test fixtures for seeding

## Future Enhancements

- **Event Sourcing** — Domain events → event store
- **Microservices** — Extract Workers to separate service
- **API Versioning** — Multiple API versions (v1, v2)
- **Observability** — Structured logging, distributed tracing, metrics
- **GraphQL** — Alternative query layer alongside REST
- **Audit Logging** — Every change tracked per specification

## References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft Architectural Patterns](https://learn.microsoft.com/en-us/dotnet/architecture/)
- [CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
