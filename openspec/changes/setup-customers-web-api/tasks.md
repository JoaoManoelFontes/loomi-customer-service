## 1. Solution and API Project

- [x] 1.1 Create or update the repository solution file for the customer service.
- [x] 1.2 Create `src/CustomerService.Api` as an ASP.NET Core Web API project targeting .NET 8 or later.
- [x] 1.3 Add the API project to the solution so it opens cleanly in Rider.
- [x] 1.4 Remove template endpoints that are not part of this first task.

## 2. Health Endpoint

- [x] 2.1 Implement `GET /health` as the only required initial endpoint.
- [x] 2.2 Return a successful HTTP status code and a small response body indicating the customer service is healthy.
- [x] 2.3 Ensure the API starts without requiring an active database connection.

## 3. Environment and Repository Hygiene

- [x] 3.1 Add `.env.example` with safe placeholder values for API ports and Postgres configuration.
- [x] 3.2 Add or update `.gitignore` to exclude build outputs, IDE noise, local environment files, and user-specific files.
- [x] 3.3 Add `.dockerignore` to keep build outputs, Git metadata, IDE files, and local environment files out of Docker build context.

## 4. Docker Support

- [x] 4.1 Add a multi-stage API `Dockerfile` that builds from the repository root and publishes only the API project.
- [x] 4.2 Add `docker-compose.yml` with `customer-api` and `postgres` services.
- [x] 4.3 Configure the API service to receive database configuration through environment variables.
- [x] 4.4 Configure the Postgres service with local port mapping, database name, user, password, persistent volume, and healthcheck.

## 5. Verification

- [x] 5.1 Run `dotnet restore`.
- [x] 5.2 Run `dotnet build`.
- [x] 5.3 Run the API locally and verify `GET /health`.
- [ ] 5.4 Build or start the Docker Compose stack and verify the API and Postgres services start.
