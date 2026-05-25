## Why

The API needs a pragmatic security foundation for the banking challenge so protected operations can distinguish administrators, customers, and internal services. This change adds simple JWT authentication and role-based authorization without introducing ASP.NET Identity or an external identity provider.

## What Changes

- Add a `User` domain entity separate from `Customer`, using CPF and password credentials for authentication.
- Add `UserRole` values for `Admin`, `Customer`, and `Service`.
- Add login support at `POST /api/v1/auth/login` that validates CPF/password and returns a JWT bearer token.
- Add an admin-only user creation endpoint at `POST /api/v1/users`.
- Persist users in SQL Server with unique CPF, nullable `customer_id`, hashed passwords, and role stored as a string.
- Configure JWT bearer authentication, authorization middleware, role policies, and Swagger bearer token support.
- Add a development/demo admin seed for CPF `00000000000` with password `Admin@123` that is not duplicated.
- Add validation and consistent error handling for auth and user creation scenarios.

## Capabilities

### New Capabilities

- `simple-auth-rbac`: Covers CPF/password login, JWT issuance, role-based authorization, admin user creation, user persistence, Swagger bearer support, and development admin seeding.

### Modified Capabilities

None.

## Impact

- Domain: new `User` entity and `UserRole` enum.
- Application: auth and user creation commands/handlers, DTOs, validators, and abstractions for user repository, password hashing, and JWT generation.
- Infrastructure: EF Core user mapping/repository, password hasher, JWT generator, database migration, and development admin seed.
- API: versioned auth/users controllers, JWT authentication/authorization registration, Swagger security definition, and protected admin endpoint.
- Tests: unit tests for domain/application behavior and integration coverage for login, authorization, duplicate CPF, and admin-only user creation.
