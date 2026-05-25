## Why

The project needs a small, executable foundation before customer business features are added. Starting with a minimal ASP.NET Core Web API, a health endpoint, and local container infrastructure gives the team a repeatable baseline for Rider, terminal, and future CI work.

## What Changes

- Create an ASP.NET Core Web API setup for the customer service.
- Expose a single initial `GET /health` endpoint that confirms the API is running.
- Add local environment configuration through `.env.example`.
- Add Docker support with a multi-stage API `Dockerfile` and `.dockerignore`.
- Add repository hygiene files including `.gitignore`.
- Add Docker Compose configuration for the API and a Postgres database service.
- Configure the API to receive its database connection information from environment variables, ready for later persistence integration.
- Keep customer domain endpoints, authentication, messaging, caching, and transfer flows out of scope for this first task.

## Capabilities

### New Capabilities
- `customers-api-foundation`: Covers the initial executable customer-service API foundation, health endpoint, container setup, environment files, and Postgres service readiness.

### Modified Capabilities

None.

## Impact

- Affects the repository root with solution/project files and infrastructure files.
- Adds the initial API project under `src/CustomerService.Api`.
- Adds Docker-based local runtime dependencies through `docker-compose.yml`.
- Introduces Postgres as the initial relational database service for future API integration.
