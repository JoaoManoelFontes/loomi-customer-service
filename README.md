# CustomerService

Initial setup for the customer-service ASP.NET Core Web API.

The service currently exposes `GET /health` and includes a Clean Architecture foundation with a minimal Customer domain model and PostgreSQL persistence through EF Core + Npgsql.

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

## Docker Compose

Create a local `.env` from `.env.example` if you want to override defaults, then run:

```bash
docker compose up -d --build
```

Local URLs:

- API: `http://localhost:5001/health`
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

The initial migration creates the `customers` table with `id`, `name`, `email`, `address`, and `profile_picture_url`.

## Tests

```bash
dotnet test
```

## Configuration

The API is prepared to receive the future Postgres connection string through:

```text
ConnectionStrings__Postgres
```

The development connection string points at the local Docker Compose Postgres service.

## Scope

Implemented in this setup:

- ASP.NET Core Web API project
- `GET /health`
- Clean Architecture projects for Domain, Application, and Infrastructure
- Minimal `Customer` domain model
- EF Core `CustomerDbContext` with Npgsql provider
- Initial Customer table migration
- Customer model unit tests
- `.env.example`
- `.gitignore`
- `.dockerignore`
- Multi-stage API `Dockerfile`
- `docker-compose.yml` with API and Postgres
- Rider-compatible solution file

Not implemented yet:

- Customer REST endpoints
- Authentication
- Messaging
- Caching
