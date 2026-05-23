## ADDED Requirements

### Requirement: Customer API project is runnable
The system SHALL include an ASP.NET Core Web API project for the customer service that can be restored, built, and run through the .NET CLI and opened from the repository solution in Rider.

#### Scenario: Build succeeds
- **WHEN** a developer runs `dotnet build` from the repository root
- **THEN** the solution builds successfully

#### Scenario: API starts locally
- **WHEN** a developer runs the API project from the .NET CLI or Rider
- **THEN** the API starts without requiring a local database connection

### Requirement: Health endpoint is exposed
The system SHALL expose a `GET /health` endpoint as the only required initial API endpoint.

#### Scenario: Health check succeeds
- **WHEN** a client sends `GET /health` to the running API
- **THEN** the API responds with a successful HTTP status code

#### Scenario: Health response identifies service status
- **WHEN** a client sends `GET /health` to the running API
- **THEN** the response body indicates that the customer service is healthy

### Requirement: Environment files are prepared
The system SHALL include safe local environment configuration examples without committing production secrets.

#### Scenario: Example environment is available
- **WHEN** a developer inspects the repository root
- **THEN** `.env.example` exists with placeholder values for API and Postgres local configuration

#### Scenario: Real local environment file is ignored
- **WHEN** a developer creates a `.env` file from `.env.example`
- **THEN** the repository ignore rules exclude `.env` from source control

### Requirement: Docker image build is supported
The system SHALL include Docker build configuration for the customer API.

#### Scenario: API image can be built
- **WHEN** a developer runs `docker build` from the repository root using the API Dockerfile
- **THEN** Docker builds an image for the ASP.NET Core API

#### Scenario: Build context excludes generated files
- **WHEN** Docker builds the API image
- **THEN** `.dockerignore` excludes local build outputs, IDE files, Git metadata, and local environment files from the build context

### Requirement: Docker Compose provides API and Postgres
The system SHALL include Docker Compose configuration with an API service and a Postgres service ready for local development.

#### Scenario: Compose starts local services
- **WHEN** a developer runs `docker compose up -d`
- **THEN** Docker Compose starts the customer API service and the Postgres service

#### Scenario: API receives database configuration
- **WHEN** the API starts through Docker Compose
- **THEN** the API service receives a Postgres connection string or equivalent Postgres configuration through environment variables

#### Scenario: Postgres is reachable locally
- **WHEN** Docker Compose starts the Postgres service
- **THEN** Postgres exposes a local port and uses configured database, user, and password values from environment variables
