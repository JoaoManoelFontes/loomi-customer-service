# CustomerService

Initial setup for the customer-service ASP.NET Core Web API with simple JWT authentication and role-based authorization.

The service exposes `GET /health`, `POST /api/v1/auth/login`, and admin-only `POST /api/v1/users`. It includes a Clean Architecture foundation with Customer and User domain models and PostgreSQL persistence through EF Core + Npgsql.

## Requirements

- .NET SDK 10.0 or later installed locally
- EF Core CLI tool for migrations: `dotnet tool install --global dotnet-ef`
- Docker Desktop for running the Compose stack

## Run Locally

```bash
dotnet restore
dotnet build
dotnet run --project src/CustomerService.Api --urls http://localhost:5001
```

Health endpoint:

```http
GET http://localhost:5001/health
```

Expected response:

```json
{
  "service": "customer-service",
  "status": "healthy"
}
```

The response also includes a UTC timestamp.

Swagger is available in Development at:

```text
http://localhost:5001/swagger
```

The API applies migrations and seeds the development admin user on Development startup, so start Postgres before running the API locally.

## Docker Compose

Create a local `.env` from `.env.example` if you want to override defaults, then run:

```bash
docker compose up -d --build
```

Local URLs:

- API: `http://localhost:5001/health`
- Swagger: `http://localhost:5001/swagger`
- Postgres: `localhost:5432`

## Database Migrations

Start Postgres before applying migrations:

```bash
docker compose up -d postgres
```

Create a new migration from the repository root:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/CustomerService.Infrastructure \
  --startup-project src/CustomerService.Api \
  --context CustomerDbContext \
  --output-dir Persistence/Migrations
```

Apply migrations locally:

```bash
dotnet ef database update \
  --project src/CustomerService.Infrastructure \
  --startup-project src/CustomerService.Api \
  --context CustomerDbContext
```

The migrations create `customers`, `banking_details`, and `users`. The `users` table stores CPF, password hash, role, optional customer id, and timestamps. CPF has a unique index.

## Authentication and RBAC

Authentication uses CPF and password. `User` is separate from `Customer`: `User` owns credentials and access control; `Customer` owns profile and banking data.

Roles:

- `Admin`: can create users and access administrative endpoints.
- `Customer`: regular authenticated customer.
- `Service`: reserved for internal service-to-service authentication.

Development/demo admin:

```text
CPF: 00000000000
Password: Admin@123
Role: Admin
```

This credential is for local development and the challenge demo only. Do not reuse it in production.

Login:

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "cpf": "00000000000",
  "password": "Admin@123"
}
```

Use the returned `accessToken` as a bearer token in Swagger or HTTP clients:

```text
Authorization: Bearer <token>
```

Create a user as Admin:

```http
POST /api/v1/users
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "cpf": "12345678900",
  "password": "Customer@123",
  "role": "Customer",
  "customerId": null
}
```

## Tests

```bash
dotnet test
```

## Configuration

The API is prepared to receive the future Postgres connection string through:

```text
ConnectionStrings__Postgres
```

JWT configuration:

```text
Jwt__Issuer
Jwt__Audience
Jwt__Secret
Jwt__ExpiresInMinutes
```

The development connection string points at the local Docker Compose Postgres service.

## Scope

Implemented in this setup:

- ASP.NET Core Web API project
- `GET /health`
- Clean Architecture projects for Domain, Application, and Infrastructure
- Minimal `Customer` domain model
- Separate `User` domain model with CPF credentials and RBAC roles
- JWT bearer login at `POST /api/v1/auth/login`
- Admin-only user creation at `POST /api/v1/users`
- Swagger bearer token support
- EF Core `CustomerDbContext` with Npgsql provider
- Customer, banking details, and user table migrations
- Customer/User domain tests plus auth application and HTTP integration tests
- `.env.example`
- `.gitignore`
- `.dockerignore`
- Multi-stage API `Dockerfile`
- `docker-compose.yml` with API and Postgres
- Rider-compatible solution file

Not implemented yet:

- Customer REST endpoints
- Messaging
- Caching
