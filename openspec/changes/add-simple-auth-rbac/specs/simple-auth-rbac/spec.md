## ADDED Requirements

### Requirement: User credentials are separate from customer profile data
The system SHALL model authentication and authorization with a `User` entity that is separate from `Customer`.

#### Scenario: User has access-control fields
- **WHEN** a user is represented by the domain model
- **THEN** the user includes an id, CPF, password hash, role, optional customer id, created timestamp, and optional updated timestamp

#### Scenario: Customer email is not used for authentication
- **WHEN** a user authenticates
- **THEN** the system uses CPF and password, not customer email

#### Scenario: Password hash is not exposed
- **WHEN** the API returns user-related responses
- **THEN** the response does not include password hash or plain text password

### Requirement: User roles are supported
The system SHALL support `Admin`, `Customer`, and `Service` roles for role-based authorization.

#### Scenario: Admin role allows administrative actions
- **WHEN** an authenticated user has the `Admin` role
- **THEN** the user can call admin-only endpoints

#### Scenario: Non-admin roles cannot call admin-only endpoints
- **WHEN** an authenticated user has the `Customer` or `Service` role
- **THEN** the user cannot call admin-only endpoints

### Requirement: Users are persisted in the relational database
The system SHALL persist users in SQL Server with unique CPF, hashed passwords, string roles, nullable customer links, and timestamps.

#### Scenario: User table is created
- **WHEN** database migrations are applied
- **THEN** a `users` table exists with `id`, `cpf`, `password_hash`, `role`, `customer_id`, `created_at`, and `updated_at` columns

#### Scenario: CPF is unique
- **WHEN** two users are persisted with the same normalized CPF
- **THEN** the database rejects the duplicate CPF

#### Scenario: Customer link is optional
- **WHEN** an Admin or Service user is persisted without a customer id
- **THEN** the user record is valid

### Requirement: Users can login with CPF and password
The system SHALL expose `POST /api/v1/auth/login` for CPF/password authentication.

#### Scenario: Successful login returns token response
- **WHEN** a request contains a valid CPF and password for an existing user
- **THEN** the API returns an access token, token type `Bearer`, and expiration in seconds

#### Scenario: Invalid login is generic
- **WHEN** a request contains an unknown CPF or invalid password
- **THEN** the API returns a failure with the message `Invalid CPF or password.`

#### Scenario: Login input is validated
- **WHEN** a login request omits CPF or password
- **THEN** the API returns a validation error

### Requirement: JWT tokens contain access-control claims
The system SHALL generate JWT bearer tokens containing user identity and role claims.

#### Scenario: Token contains required claims
- **WHEN** login succeeds
- **THEN** the JWT contains `sub`, `cpf`, and `role` claims

#### Scenario: Token contains customer claim only when linked
- **WHEN** login succeeds for a user with a customer id
- **THEN** the JWT contains a `customer_id` claim

#### Scenario: Token omits customer claim when unlinked
- **WHEN** login succeeds for a user without a customer id
- **THEN** the JWT does not contain a `customer_id` claim

### Requirement: Admin users can create users
The system SHALL expose `POST /api/v1/users` as an admin-only endpoint for creating users.

#### Scenario: Admin creates user
- **WHEN** an authenticated Admin submits valid CPF, password, role, and optional customer id
- **THEN** the API creates the user and returns id, CPF, role, and customer id

#### Scenario: Created password is hashed
- **WHEN** a user is created
- **THEN** the persisted password value is a hash and the plain text password is not persisted

#### Scenario: Duplicate CPF is rejected
- **WHEN** an Admin submits a CPF already assigned to an existing user
- **THEN** the API returns a duplicate CPF error

#### Scenario: Non-admin cannot create user
- **WHEN** an authenticated non-admin calls `POST /api/v1/users`
- **THEN** the API rejects the request as forbidden

#### Scenario: Anonymous caller cannot create user
- **WHEN** an unauthenticated caller calls `POST /api/v1/users`
- **THEN** the API rejects the request as unauthorized

### Requirement: User creation input is validated
The system SHALL validate user creation requests before creating users.

#### Scenario: Required fields are enforced
- **WHEN** a create-user request omits CPF, password, or role
- **THEN** the API returns validation errors

#### Scenario: Password length is enforced
- **WHEN** a create-user request contains a password shorter than the configured minimum
- **THEN** the API returns a validation error

#### Scenario: Role value is validated
- **WHEN** a create-user request contains an unsupported role value
- **THEN** the API returns a validation error

### Requirement: Development admin is seeded idempotently
The system SHALL create a development/demo admin user with CPF `00000000000`, password `Admin@123`, and role `Admin` when no user with that CPF exists.

#### Scenario: Seed creates initial admin
- **WHEN** the application initializes against a database without the seed CPF
- **THEN** the system creates the demo Admin user

#### Scenario: Seed does not duplicate admin
- **WHEN** the application initializes against a database that already contains the seed CPF
- **THEN** the system does not create another user with that CPF

### Requirement: API authenticates and authorizes JWT bearer tokens
The system SHALL configure JWT bearer authentication and authorization middleware for protected endpoints.

#### Scenario: Valid bearer token authorizes request
- **WHEN** a protected endpoint receives a valid bearer token with the required role
- **THEN** the request is allowed

#### Scenario: Missing bearer token is unauthorized
- **WHEN** a protected endpoint receives no bearer token
- **THEN** the API returns unauthorized

#### Scenario: Authorization middleware order is correct
- **WHEN** the API pipeline is configured
- **THEN** authentication runs before authorization

### Requirement: Swagger supports bearer authentication
The system SHALL configure Swagger/OpenAPI so callers can authorize with a JWT bearer token.

#### Scenario: Bearer security scheme is visible
- **WHEN** Swagger is opened in development
- **THEN** the OpenAPI document includes a bearer token security scheme

#### Scenario: Admin endpoint can be called from Swagger
- **WHEN** a valid Admin token is entered in Swagger authorization
- **THEN** Swagger can call `POST /api/v1/users`

### Requirement: Auth errors are consistent and safe
The system SHALL return consistent ProblemDetails-style responses for authentication, authorization, validation, duplicate CPF, and invalid credential failures.

#### Scenario: Login does not reveal account existence
- **WHEN** login fails because CPF does not exist or password is wrong
- **THEN** the response uses the same generic error message

#### Scenario: Error response omits stack traces
- **WHEN** an auth or user creation error is returned
- **THEN** the response does not expose stack traces to the client
