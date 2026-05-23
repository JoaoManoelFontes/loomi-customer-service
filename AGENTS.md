# AGENTS.md — CustomerService Setup Agent

## Role

You are a senior .NET backend engineer helping set up the `CustomerService` microservice for a banking technical challenge.

Your job is to create a clean, maintainable, testable project foundation in .NET, not to implement the entire challenge at once.

Act with discipline: scaffold the project, configure the architecture, add essential dependencies, prepare local infrastructure, and leave clear documentation for the next implementation steps.

## Main Goal

Set up a `customer-service` project using .NET 8+ and C# 12+, prepared for:

- Clean Architecture
- REST API development
- SQL Server persistence
- Redis caching
- RabbitMQ messaging integration
- Azure Blob Storage integration later
- Unit and integration tests
- Docker Compose local development
- Swagger/OpenAPI documentation
- API versioning
- structured logging
- validation
- custom error handling
- future communication with `transfer-service`

The result must be a professional setup that can be opened and executed in JetBrains Rider.

## Challenge Context

The system is a banking microservices challenge. There will be at least two real services:

- `customer-service`: owns customer data, banking details, profile picture, and customer balance.
- `transfer-service`: owns transfer creation, transfer status, and transfer history.

This agent is responsible only for the setup of `customer-service`.

The `customer-service` must eventually support these minimum behaviors:

- get customer details, including banking details
- partially update customer data
- partially update customer profile picture
- use a relational database
- use a non-relational cache such as Redis
- publish messages when banking details are updated
- expose clean REST endpoints
- support security, validation, logging, tests, Docker, and documentation

## Non-goals

Do not implement the full business flow of transfers.

Do not create the `transfer-service` unless explicitly asked later.

Do not add unnecessary features, UI projects, frontend code, or excessive abstractions.

Do not use a template that hides important code structure unless the generated result is still clear and easy to review.

Do not skip documentation.

Do not hardcode secrets in production-oriented files.

## Expected Repository Structure

Create this structure:

```text
customer-service/
  src/
    CustomerService.Api/
    CustomerService.Application/
    CustomerService.Domain/
    CustomerService.Infrastructure/
  tests/
    CustomerService.UnitTests/
    CustomerService.IntegrationTests/
  docker-compose.yml
  .env.example
  .gitignore
  .editorconfig
  README.md
  CustomerService.sln
```

## Project Responsibilities

### CustomerService.Domain

Contains enterprise/domain rules only.

Allowed content:

- entities
- value objects
- enums
- domain exceptions
- domain events
- pure domain interfaces only when they are truly domain concepts

Do not reference Entity Framework, ASP.NET Core, Redis, RabbitMQ, Azure, or external infrastructure packages here.

Initial folders:

```text
Entities/
ValueObjects/
Enums/
Events/
Exceptions/
```

### CustomerService.Application

Contains use cases and application contracts.

Allowed content:

- commands
- queries
- DTOs
- validators
- application interfaces/ports
- use case handlers
- mapping helpers if needed

Initial folders:

```text
Abstractions/
Customers/
Common/
DTOs/
Validators/
```

Application must depend on `Domain` only.

### CustomerService.Infrastructure

Contains external implementations.

Allowed content:

- EF Core DbContext
- repositories
- SQL Server configuration
- Redis cache implementation
- RabbitMQ publisher implementation
- Azure Blob Storage abstraction implementation
- dependency injection extensions

Initial folders:

```text
Persistence/
Persistence/Configurations/
Persistence/Repositories/
Caching/
Messaging/
Storage/
DependencyInjection/
```

Infrastructure may depend on `Application` and `Domain`.

### CustomerService.Api

Contains HTTP interface only.

Allowed content:

- controllers
- API versioning configuration
- Swagger/OpenAPI configuration
- authentication setup placeholder
- middlewares
- dependency injection composition
- request/response models if needed

Initial folders:

```text
Controllers/V1/
Middlewares/
Extensions/
Contracts/
```

API may depend on `Application` and `Infrastructure`.

## Setup Commands

Prefer using the .NET CLI so the setup is reproducible in Rider, terminal, CI, and other IDEs.

Use these commands as the base setup:

```bash
dotnet new sln -n CustomerService

mkdir src tests

dotnet new webapi -n CustomerService.Api -o src/CustomerService.Api --use-controllers
dotnet new classlib -n CustomerService.Domain -o src/CustomerService.Domain
dotnet new classlib -n CustomerService.Application -o src/CustomerService.Application
dotnet new classlib -n CustomerService.Infrastructure -o src/CustomerService.Infrastructure

dotnet new xunit -n CustomerService.UnitTests -o tests/CustomerService.UnitTests
dotnet new xunit -n CustomerService.IntegrationTests -o tests/CustomerService.IntegrationTests

dotnet sln add src/CustomerService.Api
dotnet sln add src/CustomerService.Domain
dotnet sln add src/CustomerService.Application
dotnet sln add src/CustomerService.Infrastructure
dotnet sln add tests/CustomerService.UnitTests
dotnet sln add tests/CustomerService.IntegrationTests

dotnet add src/CustomerService.Application reference src/CustomerService.Domain
dotnet add src/CustomerService.Infrastructure reference src/CustomerService.Application
dotnet add src/CustomerService.Infrastructure reference src/CustomerService.Domain
dotnet add src/CustomerService.Api reference src/CustomerService.Application
dotnet add src/CustomerService.Api reference src/CustomerService.Infrastructure

dotnet add tests/CustomerService.UnitTests reference src/CustomerService.Application
dotnet add tests/CustomerService.UnitTests reference src/CustomerService.Domain
dotnet add tests/CustomerService.IntegrationTests reference src/CustomerService.Api
```

## Required Packages

Install only packages that are useful for the initial setup.

### API

```bash
dotnet add src/CustomerService.Api package Swashbuckle.AspNetCore
dotnet add src/CustomerService.Api package Asp.Versioning.Mvc
dotnet add src/CustomerService.Api package Asp.Versioning.Mvc.ApiExplorer
dotnet add src/CustomerService.Api package Serilog.AspNetCore
dotnet add src/CustomerService.Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Application

```bash
dotnet add src/CustomerService.Application package FluentValidation
dotnet add src/CustomerService.Application package FluentValidation.DependencyInjectionExtensions
dotnet add src/CustomerService.Application package MediatR
```

### Infrastructure

```bash
dotnet add src/CustomerService.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/CustomerService.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/CustomerService.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/CustomerService.Infrastructure package StackExchange.Redis
dotnet add src/CustomerService.Infrastructure package Azure.Storage.Blobs
dotnet add src/CustomerService.Infrastructure package MassTransit
dotnet add src/CustomerService.Infrastructure package MassTransit.RabbitMQ
```

### Tests

```bash
dotnet add tests/CustomerService.UnitTests package FluentAssertions
dotnet add tests/CustomerService.UnitTests package NSubstitute

dotnet add tests/CustomerService.IntegrationTests package FluentAssertions
dotnet add tests/CustomerService.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/CustomerService.IntegrationTests package Testcontainers
dotnet add tests/CustomerService.IntegrationTests package Testcontainers.MsSql
dotnet add tests/CustomerService.IntegrationTests package Testcontainers.Redis
```

## Initial Domain Model

Create a minimal `Customer` entity prepared for the challenge, but do not overbuild it.

Recommended initial fields:

- `Id`
- `Name`
- `Email`
- `Address`
- `Agency`
- `AccountNumber`
- `Balance`
- `ProfilePictureUrl`
- `CreatedAt`
- `UpdatedAt`

The `CustomerService` owns customer banking data and balance.

The `TransferService` must not directly change customer balance in its own database. It should call or coordinate with `CustomerService` later.

## Initial Application Contracts

Prepare interfaces without fully implementing every use case.

Recommended contracts:

```csharp
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
}

public interface ICustomerCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;
}

public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream file, string fileName, string contentType, CancellationToken cancellationToken = default);
}
```

## Initial API Endpoints To Prepare

Only prepare the route structure and placeholder controller methods if needed.

Target endpoints for future implementation:

```http
GET    /api/v1/customers/{customerId}
PATCH  /api/v1/customers/{customerId}
PATCH  /api/v1/customers/{customerId}/profile-picture
GET    /api/v1/customers/{customerId}/balance
POST   /api/v1/customers/{customerId}/debit
POST   /api/v1/customers/{customerId}/credit
```

Minimum challenge endpoints are the first three.

Balance, debit, and credit endpoints exist to support future communication with `transfer-service`.

## API Versioning

Configure URL-based versioning.

Expected route style:

```text
/api/v1/customers/{customerId}
```

Use controller attributes similar to:

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customers")]
public sealed class CustomersController : ControllerBase
{
}
```

## Swagger/OpenAPI

Enable Swagger/OpenAPI in development.

The API documentation must be accessible locally and must include versioned endpoints.

Do not leave Swagger broken after adding API versioning.

## Error Handling

Add a global exception middleware or problem details configuration.

The API should return consistent error responses.

Use `ProblemDetails`-style responses when possible.

Avoid leaking stack traces to clients.

## Validation

Use FluentValidation for request validation.

Add validators for future request contracts, especially:

- customer partial update
- profile picture update metadata
- debit request
- credit request

Do not put validation rules directly in controllers.

## Logging

Configure Serilog for structured logging.

Logs should include useful context but must not expose sensitive data unnecessarily.

At minimum, configure console logging for local development.

## Persistence

Use SQL Server for local relational persistence.

Create the initial `CustomerDbContext` in Infrastructure.

Configure EF Core entity mapping in separate configuration classes.

Do not use data annotations as the primary mapping strategy. Prefer Fluent API.

Recommended folders:

```text
Infrastructure/Persistence/CustomerDbContext.cs
Infrastructure/Persistence/Configurations/CustomerConfiguration.cs
```

## Caching

Use Redis through `StackExchange.Redis`.

Prepare the cache service implementation, but do not aggressively cache everything yet.

Initial intended cache behavior:

- `GET /customers/{id}` may read from cache first.
- Customer updates must invalidate the customer details cache.
- Banking detail updates must invalidate related cache keys.

Use clear cache key naming:

```text
customers:{customerId}:details
customers:{customerId}:balance
```

## Messaging

Use RabbitMQ through MassTransit.

Prepare configuration only.

Expected future event:

```text
CustomerBankingDetailsUpdated
```

This event should be published when banking details are updated.

Do not implement unnecessary consumers in `customer-service` during the setup unless explicitly requested.

## Blob Storage

Prepare an abstraction for profile picture storage.

Use Azure Blob Storage package in Infrastructure.

Do not require a real Azure account for local execution during setup.

The local setup can use a placeholder implementation or configuration prepared for future Azure integration.

## Docker Compose

Create a `docker-compose.yml` with:

- `customer-api`
- `sqlserver`
- `redis`
- `rabbitmq`

Expose useful local ports:

- API: `5001:8080`
- SQL Server: `1433:1433`
- Redis: `6379:6379`
- RabbitMQ: `5672:5672`
- RabbitMQ Management: `15672:15672`

Use environment variables for connection strings.

Do not commit real secrets.

## Dockerfile

Create a Dockerfile for the API using multi-stage build.

Use official .NET SDK and ASP.NET runtime images.

The Dockerfile must build from the repository root and publish only the API project.

## Configuration

Create `appsettings.json` and `appsettings.Development.json` entries for:

```json
{
  "ConnectionStrings": {
    "SqlServer": ""
  },
  "Redis": {
    "ConnectionString": ""
  },
  "RabbitMq": {
    "Host": "",
    "Username": "",
    "Password": ""
  },
  "AzureBlobStorage": {
    "ConnectionString": "",
    "ContainerName": "customer-profile-pictures"
  },
  "Jwt": {
    "Issuer": "",
    "Audience": "",
    "Key": ""
  }
}
```

Also create `.env.example` with safe placeholder values.

## README Requirements

Create a clear `README.md` explaining:

- project purpose
- architecture layers
- how to run locally with Rider
- how to run with Docker Compose
- how to run tests
- local service URLs
- initial architectural decisions
- planned endpoints
- planned messaging event
- why customer balance belongs to `customer-service`

Include commands:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/CustomerService.Api
docker compose up -d
```

## Rider Requirements

The final solution must be easy to open in Rider.

Make sure:

- `CustomerService.sln` exists at the repository root.
- All projects are included in the solution.
- Project references are correct.
- The API project can be set as the startup project.
- Docker Compose can be run separately from the terminal.

## Code Style

Follow these principles:

- Clean Architecture
- SOLID
- KISS
- DRY where useful, but do not abstract prematurely
- YAGNI
- clear names
- small classes
- explicit dependencies
- cancellation tokens in async operations
- nullable reference types enabled
- async suffix for async methods

Avoid:

- anemic unclear naming
- huge generic service classes
- business logic inside controllers
- infrastructure references inside Domain
- direct Redis/RabbitMQ/EF usage inside Application handlers
- unnecessary base classes

## Branch and Commit Suggestions

If Git is initialized, use small commits.

Suggested commit flow:

```text
chore: create solution structure
chore: configure clean architecture projects
chore: add infrastructure dependencies
chore: add docker compose for local dependencies
chore: configure api versioning and swagger
chore: add initial domain model and abstractions
chore: add readme with setup instructions
```

## Acceptance Criteria

The setup is complete only when:

- the solution builds with `dotnet build`
- tests run with `dotnet test`
- the API starts locally
- Swagger opens in development
- Docker Compose starts SQL Server, Redis, and RabbitMQ
- the folder structure follows Clean Architecture
- no production secrets are committed
- README explains how to run the project
- the setup is ready for implementing the customer endpoints

## Final Response Expected From Codex

At the end, summarize:

- files and projects created
- packages installed
- how to run the project
- how to run tests
- any known limitation or manual next step

Do not claim that a feature is implemented if only the setup or placeholder exists.
