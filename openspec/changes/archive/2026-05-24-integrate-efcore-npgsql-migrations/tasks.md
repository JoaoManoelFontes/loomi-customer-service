## 1. Solution Structure

- [x] 1.1 Create `src/CustomerService.Domain`, `src/CustomerService.Application`, `src/CustomerService.Infrastructure`, and `tests/CustomerService.UnitTests` projects if they are missing.
- [x] 1.2 Add the new projects to `CustomerService.sln` under appropriate solution folders.
- [x] 1.3 Add project references: Application -> Domain, Infrastructure -> Application and Domain, API -> Infrastructure, UnitTests -> Domain.
- [x] 1.4 Create initial folders for `Domain/Entities`, `Domain/Exceptions`, `Infrastructure/Persistence`, `Infrastructure/Persistence/Configurations`, `Infrastructure/Persistence/Migrations`, and `Infrastructure/DependencyInjection`.

## 2. Dependencies and Configuration

- [x] 2.1 Add `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design`, and `Npgsql.EntityFrameworkCore.PostgreSQL` to `CustomerService.Infrastructure`.
- [x] 2.2 Add `FluentAssertions` or equivalent assertion package to `CustomerService.UnitTests`.
- [x] 2.3 Add or update `ConnectionStrings:Postgres` in API configuration files with safe local placeholder values.
- [x] 2.4 Update `.env.example` if needed so local PostgreSQL values line up with the documented connection string.

## 3. Domain Model

- [x] 3.1 Implement `CustomerService.Domain.Entities.Customer` with identity, name, email, address, and optional profile picture URL.
- [x] 3.2 Add domain validation for required name, email, and address values.
- [x] 3.3 Add methods for updating profile data and profile picture URL while preserving validation rules.
- [x] 3.4 Add a small domain exception type for invalid Customer state if one does not already exist.

## 4. EF Core Persistence

- [x] 4.1 Implement `CustomerDbContext` with a `DbSet<Customer>`.
- [x] 4.2 Implement `CustomerConfiguration` using Fluent API to map `Customer` to `customers`.
- [x] 4.3 Configure snake_case columns: `id`, `name`, `email`, `address`, and `profile_picture_url`.
- [x] 4.4 Configure required columns and maximum lengths for `name`, `email`, and `address`; allow null for `profile_picture_url`.
- [x] 4.5 Add an Infrastructure DI extension that registers `CustomerDbContext` with Npgsql using `ConnectionStrings:Postgres`.
- [x] 4.6 Update API startup to call the Infrastructure registration without moving persistence details into `Program.cs`.

## 5. Migration

- [x] 5.1 Install or use the EF Core CLI tooling needed to create migrations.
- [x] 5.2 Generate an initial migration in `CustomerService.Infrastructure/Persistence/Migrations`.
- [x] 5.3 Verify the migration creates the `customers` table with the expected columns and constraints.
- [x] 5.4 Document the local commands for creating and applying migrations.

## 6. Unit Tests

- [x] 6.1 Add Customer model tests for successful creation with valid name, email, and address.
- [x] 6.2 Add Customer model tests for optional profile picture URL behavior.
- [x] 6.3 Add Customer model tests for rejecting empty name, email, and address.
- [x] 6.4 Add Customer model tests for updating profile data and profile picture URL.
- [x] 6.5 Add Customer model tests for rejecting invalid profile updates.

## 7. Verification

- [x] 7.1 Run `dotnet restore`.
- [x] 7.2 Run `dotnet build`.
- [x] 7.3 Run `dotnet test`.
- [x] 7.4 Start PostgreSQL with Docker Compose and apply the migration locally.
- [x] 7.5 Confirm the database contains the `customers` table after migration.
