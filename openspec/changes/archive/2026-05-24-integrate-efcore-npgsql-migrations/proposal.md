## Why

The service needs a real persistence foundation before customer endpoints can be implemented safely. Adding EF Core with Npgsql and an initial Customer table now gives the project a concrete database model, migration workflow, and domain model tests without expanding into full customer use cases yet.

## What Changes

- Add a Clean Architecture-oriented folder structure for domain, application, infrastructure, and tests if missing.
- Introduce a minimal `Customer` domain model containing `name`, `email`, `address`, and `profilePictureUrl`.
- Configure EF Core with the Npgsql provider for PostgreSQL.
- Add a `CustomerDbContext` and explicit Fluent API mapping for the `customers` table.
- Create an initial migration that creates the `customers` table.
- Wire infrastructure dependency injection so the API can register PostgreSQL persistence from configuration.
- Add unit tests for the `Customer` model behavior and validation invariants.
- Update local configuration and documentation as needed for running migrations.

## Capabilities

### New Capabilities
- `customer-postgres-persistence`: Covers PostgreSQL persistence for customers through EF Core, Npgsql configuration, migrations, and the initial Customer table schema.
- `customer-domain-model`: Covers the minimal Customer domain model and its unit-tested invariants.

### Modified Capabilities

None.

## Impact

- Affects `src/CustomerService.Domain` for the `Customer` model.
- Affects `src/CustomerService.Infrastructure` for EF Core, Npgsql, DbContext, entity configuration, migrations, and dependency injection.
- Affects `src/CustomerService.Api` for persistence registration and configuration usage.
- Affects `tests/CustomerService.UnitTests` for Customer model tests.
- Adds or updates NuGet dependencies for EF Core, Npgsql provider, design-time tooling, xUnit, and assertions as needed.
- Requires PostgreSQL connection string configuration for local migration execution.
