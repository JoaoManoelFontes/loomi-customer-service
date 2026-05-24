## 1. Domain Model

- [x] 1.1 Add `UserRole` enum with `Admin`, `Customer`, and `Service` values.
- [x] 1.2 Add `User` entity separated from `Customer`, including CPF, password hash, role, optional customer id, and timestamps.
- [x] 1.3 Add domain-level guards or factory/update methods so CPF and password hash cannot be empty.
- [x] 1.4 Add unit tests for user creation, optional customer link, role assignment, and password hash encapsulation.

## 2. Application Contracts and Use Cases

- [x] 2.1 Add `IUserRepository`, `IPasswordHasher`, and `IJwtTokenGenerator` abstractions in Application.
- [x] 2.2 Add login request/response models, command/query handler, and generic invalid-credentials error.
- [x] 2.3 Add create-user request/response models, command handler, duplicate CPF handling, and password hashing flow.
- [x] 2.4 Add FluentValidation validators for login and create-user requests.
- [x] 2.5 Normalize CPF consistently before lookup and persistence.
- [x] 2.6 Add unit tests for login success, login failure, duplicate CPF, password hashing, and create-user response shape.

## 3. Infrastructure Persistence and Auth Services

- [x] 3.1 Add `DbSet<User>` to `CustomerDbContext`.
- [x] 3.2 Add EF Core `UserConfiguration` mapping to the `users` table with unique CPF index, string role conversion, nullable `customer_id`, and timestamps.
- [x] 3.3 Implement `UserRepository` with lookup by CPF and add/update operations needed by the use cases.
- [x] 3.4 Implement `PasswordHasher` that stores non-reversible hashes and verifies supplied passwords.
- [x] 3.5 Implement `JwtTokenGenerator` using configured issuer, audience, secret, and expiration.
- [x] 3.6 Add a migration for the `users` table and unique CPF index.
- [x] 3.7 Add idempotent development admin seed for CPF `00000000000`, password `Admin@123`, and role `Admin`.
- [x] 3.8 Register repository, hasher, token generator, and seed support in dependency injection.

## 4. API Endpoints and Security Configuration

- [x] 4.1 Configure controllers, API versioning, and endpoint mapping if not already active in `Program.cs`.
- [x] 4.2 Configure JWT bearer authentication from `Jwt` settings and validate required JWT options at startup.
- [x] 4.3 Configure authorization and ensure `UseAuthentication()` runs before `UseAuthorization()`.
- [x] 4.4 Add versioned `AuthController` with `POST /api/v1/auth/login`.
- [x] 4.5 Add versioned `UsersController` with `POST /api/v1/users` protected by the Admin role.
- [x] 4.6 Ensure all auth/user responses exclude password hashes and plain text passwords.
- [x] 4.7 Update Swagger/OpenAPI configuration with bearer token security scheme and protected endpoint metadata.
- [x] 4.8 Add or update ProblemDetails-style error mapping for validation, invalid credentials, duplicate CPF, unauthorized, and forbidden responses.

## 5. Configuration and Documentation

- [x] 5.1 Add `Jwt` settings for issuer, audience, secret, and expiration to `appsettings.json` and development configuration with safe placeholders.
- [x] 5.2 Update `.env.example` with JWT-related environment variable placeholders if Docker/local configuration uses environment variables.
- [x] 5.3 Document demo admin credentials, login flow, Swagger bearer usage, and role meanings in `README.md`.
- [x] 5.4 Document that the demo admin credential is for development/challenge use only and must not be reused in production.

## 6. Verification

- [x] 6.1 Add integration tests for successful login and required JWT claims.
- [x] 6.2 Add integration tests for invalid login returning generic error.
- [x] 6.3 Add integration tests proving anonymous and non-admin callers cannot create users.
- [x] 6.4 Add integration test proving an Admin token can create a user and duplicate CPF is rejected.
- [x] 6.5 Run `dotnet build` and fix compile errors.
- [x] 6.6 Run `dotnet test` and fix failing tests.
- [x] 6.7 Start the API locally and verify Swagger login plus bearer authorization manually if practical.
