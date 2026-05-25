# customer-postgres-persistence Specification

## Purpose
TBD - created by archiving change integrate-efcore-npgsql-migrations. Update Purpose after archive.
## Requirements
### Requirement: PostgreSQL persistence is configured through Infrastructure
The system SHALL configure EF Core PostgreSQL persistence in the Infrastructure layer using the Npgsql provider and a connection string from application configuration.

#### Scenario: API registers persistence
- **WHEN** the API starts with `ConnectionStrings:Postgres` configured
- **THEN** the service container includes `CustomerDbContext` configured with the Npgsql provider

#### Scenario: Infrastructure owns provider configuration
- **WHEN** the application layer is inspected
- **THEN** it MUST NOT reference EF Core, Npgsql, or persistence provider packages

### Requirement: Customer table schema is created by migration
The system SHALL include an initial EF Core migration that creates a PostgreSQL `customers` table for customer profile data.

#### Scenario: Initial migration defines customers table
- **WHEN** the initial migration is generated
- **THEN** it creates a `customers` table with columns for `id`, `name`, `email`, `address`, and `profile_picture_url`

#### Scenario: Required customer columns are constrained
- **WHEN** the `customers` table is created
- **THEN** `name`, `email`, and `address` are required columns with explicit maximum lengths

#### Scenario: Profile picture URL is optional
- **WHEN** the `customers` table is created
- **THEN** `profile_picture_url` allows null values

### Requirement: Customer mapping uses Fluent API
The system SHALL map the Customer domain entity with EF Core Fluent API configuration in Infrastructure.

#### Scenario: Mapping is separated from domain entity
- **WHEN** the Customer entity source is inspected
- **THEN** it MUST NOT contain EF Core data annotations or provider-specific attributes

#### Scenario: Mapping applies table and column names
- **WHEN** EF Core builds the Customer model
- **THEN** the entity is mapped to `customers` with snake_case database column names

### Requirement: Local migrations are executable
The system SHALL document and support local EF Core migration execution against the Docker Compose PostgreSQL database.

#### Scenario: Developer applies migration locally
- **WHEN** PostgreSQL is running from Docker Compose and the developer runs the documented `dotnet ef database update` command
- **THEN** the local database contains the `customers` table

