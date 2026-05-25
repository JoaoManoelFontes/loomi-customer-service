## Context

The repository is at the beginning of the customer-service implementation. The immediate need is not the full banking challenge, but a working foundation that can be opened in Rider, run locally, built in Docker, and later extended with customer persistence.

This change establishes the first runnable ASP.NET Core Web API surface with a single health endpoint and a Postgres database service available through Docker Compose.

## Goals / Non-Goals

**Goals:**
- Create a .NET 8+ ASP.NET Core Web API project for the customer service.
- Add a `GET /health` endpoint that returns a successful response when the API is running.
- Add repository-level `.env.example`, `.gitignore`, `.dockerignore`, `Dockerfile`, and `docker-compose.yml`.
- Configure Docker Compose with an API service and a Postgres service.
- Provide environment-variable based configuration for the future database connection.
- Keep the setup simple enough to build and run from Rider or the .NET CLI.

**Non-Goals:**
- Do not implement customer CRUD or banking endpoints yet.
- Do not implement Entity Framework mappings, migrations, repositories, or domain model yet.
- Do not add Redis, RabbitMQ, Azure Blob Storage, authentication, API versioning, or Swagger in this first task unless required by the selected API template.
- Do not store real secrets in committed files.

## Decisions

- Use ASP.NET Core Web API on .NET 8+ as the initial executable service.
  - Rationale: It matches the target backend stack and is the smallest useful foundation for later REST endpoints.
  - Alternative considered: scaffold the full Clean Architecture solution immediately. That is deferred to keep this first task scoped to a runnable service baseline.

- Implement `/health` as the only initial endpoint.
  - Rationale: A health endpoint verifies the application starts in local and containerized environments without implying customer business behavior is ready.
  - Alternative considered: add placeholder customer routes. That would expand the API contract before requirements are ready.

- Use Postgres in Docker Compose for the first database service.
  - Rationale: The user explicitly asked for Postgres ready to integrate with the API. Compose gives a repeatable local database without requiring local installation.
  - Alternative considered: SQL Server, matching earlier broader challenge notes. This change follows the current explicit request for Postgres.

- Keep the database "ready to integrate" instead of fully wired to persistence.
  - Rationale: The first task is setup only. The API can expose configuration for the connection string now, while actual data access can be added in a later change.
  - Alternative considered: add EF Core and migrations now. That is unnecessary until the first persistent customer use case exists.

- Use environment variables and `.env.example` for local configuration.
  - Rationale: This avoids committed secrets and makes Docker Compose configuration explicit.
  - Alternative considered: hardcode development credentials in project settings. That is less portable and easier to accidentally reuse outside local development.

## Risks / Trade-offs

- Minimal setup may need refactoring when Clean Architecture layers are introduced later -> Mitigation: keep endpoint and configuration code small and isolated.
- Postgres service may be running before it is ready to accept connections -> Mitigation: include a Compose healthcheck for the Postgres service.
- Docker Compose environment values are local-only and not production-grade -> Mitigation: document placeholders in `.env.example` and avoid committing real secrets.
- Skipping EF Core now means database integration is not verified through application code -> Mitigation: make this explicit in the task and add persistence in a follow-up change.
