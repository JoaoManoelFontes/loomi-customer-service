## Context

The repository currently has a runnable `CustomerService.Api` with a health endpoint and Docker Compose support for PostgreSQL. The next step is to introduce the missing Clean Architecture layers and connect the service to PostgreSQL through EF Core using the Npgsql provider.

The customer persistence scope is intentionally narrow: create the Customer domain model, map it to a `customers` table, generate the initial migration, and prove the model behavior with unit tests. Customer API endpoints, repositories, caching, messaging, and transfer coordination remain out of scope for this change.

## Goals / Non-Goals

**Goals:**
- Add `CustomerService.Domain`, `CustomerService.Application`, `CustomerService.Infrastructure`, and `CustomerService.UnitTests` projects when they are missing.
- Keep the `Customer` model in the Domain project and free of EF Core attributes or infrastructure dependencies.
- Store customers in PostgreSQL through EF Core and Npgsql.
- Use Fluent API mapping in Infrastructure instead of data annotations.
- Create an initial EF Core migration that creates the `customers` table.
- Register persistence through an Infrastructure dependency injection extension.
- Add focused unit tests for `Customer` creation and update behavior.
- Document the migration commands needed for local development.

**Non-Goals:**
- Do not implement full customer CRUD endpoints.
- Do not add Redis, RabbitMQ, Azure Blob Storage, authentication, or transfer-service communication.
- Do not add repository behavior beyond what is necessary for the persistence foundation.
- Do not put EF Core references in Domain or Application.
- Do not use SQL Server packages for this change.

## Decisions

- Use PostgreSQL with `Npgsql.EntityFrameworkCore.PostgreSQL`.
  - Rationale: The current local Compose stack already provides PostgreSQL, and the requested integration explicitly names Npgsql.
  - Alternative considered: SQL Server provider. This conflicts with the current explicit database direction for this change.

- Place `Customer` in `CustomerService.Domain/Entities`.
  - Rationale: Customer data is core business state and must not depend on EF Core or ASP.NET Core.
  - Alternative considered: define an EF-only persistence model. That would duplicate the customer concept too early and make tests less representative.

- Keep the initial Customer fields limited to `Name`, `Email`, `Address`, and `ProfilePictureUrl`, plus an internal identity needed for persistence.
  - Rationale: The user requested only those business columns for this table. The domain entity still needs an identifier so EF Core can track and persist rows.
  - Alternative considered: include banking and balance fields from the broader challenge. Those belong to later specs and would over-expand this change.

- Use Fluent API configuration under `Infrastructure/Persistence/Configurations`.
  - Rationale: It keeps database mapping concerns outside the domain model and matches Clean Architecture boundaries.
  - Alternative considered: data annotations on the entity. That would leak persistence details into Domain.

- Put migrations under `Infrastructure/Persistence/Migrations`.
  - Rationale: Infrastructure owns persistence implementation details, including schema evolution.
  - Alternative considered: migrations in the API project. That makes the API own database schema details and weakens separation.

- Register `CustomerDbContext` through `AddInfrastructure`.
  - Rationale: API composition stays explicit while the EF Core provider setup remains in Infrastructure.
  - Alternative considered: configure `DbContext` directly in `Program.cs`. This is acceptable for tiny projects but becomes noisy as infrastructure grows.

## Risks / Trade-offs

- Existing solution only contains the API project -> Add missing projects and references as part of the implementation.
- EF Core migrations require a design-time startup path -> Use the API project as startup and Infrastructure as migration assembly.
- The requested table fields omit audit columns and banking fields -> Keep the migration minimal and defer richer customer data to future specs.
- Unit tests for the domain model do not verify database mappings -> Add mapping and migration verification later through integration tests when persistence behavior is exposed.
- PostgreSQL may not be running when applying migrations -> Document `docker compose up -d postgres` before migration commands.

## Migration Plan

1. Add the Domain, Application, Infrastructure, and UnitTests projects and references.
2. Add EF Core/Npgsql dependencies to Infrastructure and test dependencies to UnitTests.
3. Implement `Customer`, `CustomerDbContext`, entity mapping, and infrastructure DI registration.
4. Configure the API to call Infrastructure registration using `ConnectionStrings:Postgres`.
5. Generate the initial migration in Infrastructure.
6. Run unit tests and build the solution.
7. Apply the migration locally against the Docker Compose PostgreSQL service.

Rollback for local development is to remove the generated migration or run `dotnet ef database drop` against the local database before regenerating the schema. Production rollback is out of scope because this service is not deployed yet.

## Open Questions

- Should the physical table name be singular `customer` or plural `customers`? This design uses `customers`, which is conventional for table collections.
- Should `profilePictureUrl` be nullable? This design allows it to be optional because a customer may exist before uploading a profile picture.
