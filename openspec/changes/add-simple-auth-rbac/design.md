## Context

`customer-service` is a Clean Architecture Web API with Domain, Application, Infrastructure, and Api projects. The service currently owns customer data and needs a small authentication boundary for the challenge so administrative setup, customer access, and future service-to-service calls can be protected without adopting a full identity platform.

Authentication must use CPF and password because `Customer.email` belongs to customer profile data, not credentials. `User` is a separate access-control model and may optionally link to a `Customer`.

## Goals / Non-Goals

**Goals:**

- Add simple CPF/password login with JWT bearer tokens.
- Add role-based authorization for `Admin`, `Customer`, and `Service`.
- Add an admin-only endpoint for creating users.
- Persist users with unique CPF, hashed passwords, string roles, nullable customer links, and timestamps.
- Seed a development/demo admin user without duplicating it.
- Keep Domain free of infrastructure dependencies and keep controllers thin.
- Make Swagger usable with bearer tokens.

**Non-Goals:**

- No ASP.NET Identity, Identity Server, OAuth2, Azure Entra ID, refresh tokens, password reset, email confirmation, or permissions table.
- No full user management CRUD.
- No complex CPF checksum validation unless a shared validation utility already exists.

## Decisions

### Separate `User` from `Customer`

`User` will live in Domain with CPF, password hash, role, optional `CustomerId`, and timestamps. `Customer` remains the profile and banking aggregate. This avoids mixing credentials with customer profile fields and keeps email out of authentication.

Alternative considered: add auth fields to `Customer`. Rejected because admin and service users may not have customers, and credentials/access control have a different lifecycle than customer profile data.

### Application owns use cases and contracts

Application will define login and create-user use cases plus `IUserRepository`, `IPasswordHasher`, and `IJwtTokenGenerator`. Handlers will coordinate validation, duplicate checks, password verification/hashing, and token generation.

Alternative considered: implement auth logic directly in API controllers. Rejected because it would put business/application decisions in the HTTP layer and make testing harder.

### Infrastructure implements persistence, hashing, token generation, and seeding

Infrastructure will add EF Core mapping for `users`, a repository implementation, a password hasher, a JWT generator, and a startup seed service or extension for the development/demo admin. Password hashing should use a framework-provided password hasher or a similarly reputable one-way hashing implementation; plain text and reversible encryption are not acceptable.

Alternative considered: seed users through static EF `HasData`. Rejected because the password hash should be generated through the same hashing service and the seed must be idempotent against existing data.

### JWT bearer with role claims

JWT tokens will include `sub`, `cpf`, `role`, and `customer_id` only when a user has a customer link. The API will configure JWT bearer authentication and role-based authorization with `UseAuthentication()` before `UseAuthorization()`.

Alternative considered: custom API keys. Rejected because the challenge explicitly calls for JWT bearer authentication and Swagger authorization with bearer tokens.

### API routes stay versioned and thin

`POST /api/v1/auth/login` and `POST /api/v1/users` will be controller endpoints. Controllers will bind request contracts, rely on FluentValidation and application handlers, and return response DTOs without password hashes.

Alternative considered: minimal endpoints for auth. Rejected to stay aligned with the existing controller-oriented setup goal and versioned API conventions.

### Errors use the existing problem-details pattern

Invalid credentials will return a generic `Invalid CPF or password.` message. Duplicate CPF, validation failures, unauthorized, and forbidden responses will be mapped to consistent ProblemDetails-style responses without stack traces.

Alternative considered: returning detailed login failures. Rejected because it leaks account existence.

## Risks / Trade-offs

- Development admin password is predictable -> keep it documented as development/demo only and require configuration changes before production.
- Simple RBAC is coarse-grained -> acceptable for the challenge; more granular permissions can be added later if requirements justify it.
- JWT secret misconfiguration can break login or weaken security -> validate required JWT options at startup and keep real secrets out of committed files.
- Startup seeding touches the database -> make seeding idempotent and limited to the configured demo admin.
- CPF normalization mistakes can allow duplicates -> normalize CPF before lookup and persistence, and enforce a unique database index.

## Migration Plan

1. Add Domain, Application, Infrastructure, and API auth/user files.
2. Add EF Core user configuration and a migration creating the `users` table with a unique CPF index.
3. Add JWT settings to appsettings and `.env.example` using safe development placeholders.
4. Register authentication, authorization, Swagger bearer security, repositories, auth services, validators, and handlers.
5. Add idempotent development admin seed during application startup or infrastructure initialization.
6. Add focused unit and integration tests for login, token claims, duplicate CPF, and admin-only user creation.

Rollback is removing the migration and auth registrations before deployment, or reverting the change before schema migration is applied.

## Open Questions

- The exact password hasher implementation should follow available package references in the project; if `Microsoft.AspNetCore.Identity` abstractions are not already available through shared framework references, a small PBKDF2-based hasher can be implemented in Infrastructure.
- The implementation should confirm the current API registration pattern because the present `Program.cs` may still be minimal and may need controller, versioning, Swagger, and middleware registration work before auth endpoints are exposed.
