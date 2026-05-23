# CustomerService

Initial setup for the customer-service ASP.NET Core Web API.

This first task intentionally keeps the service small: the API exposes only `GET /health`, while Docker and environment files prepare the project for a future Postgres integration.

## Requirements

- .NET SDK 10.0 or later installed locally
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

## Configuration

The API is prepared to receive the future Postgres connection string through:

```text
ConnectionStrings__Postgres
```

No database connection is opened during startup yet. Persistence will be added in a later task.

## Scope

Implemented in this setup:

- ASP.NET Core Web API project
- `GET /health`
- `.env.example`
- `.gitignore`
- `.dockerignore`
- Multi-stage API `Dockerfile`
- `docker-compose.yml` with API and Postgres
- Rider-compatible solution file

Not implemented yet:

- Customer domain model
- Customer REST endpoints
- EF Core DbContext and migrations
- Authentication
- Messaging
- Caching
