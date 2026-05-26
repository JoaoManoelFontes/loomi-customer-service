## 1. Cache Contracts

- [x] 1.1 Add an application abstraction for customer existence cache access with async get, set, and remove operations.
- [x] 1.2 Add a cache key helper or constant that produces `customer:{customerId}` consistently.
- [x] 1.3 Add configuration options for customer existence cache TTL.

## 2. Redis Infrastructure

- [x] 2.1 Add Redis package references if the Infrastructure project does not already include them.
- [x] 2.2 Implement the Redis-backed customer existence cache in Infrastructure.
- [x] 2.3 Register the Redis connection and customer cache implementation in Infrastructure dependency injection.
- [x] 2.4 Ensure Redis read and write failures degrade gracefully without preventing PostgreSQL fallback.

## 3. Customer Exists Flow

- [x] 3.1 Update `CustomerExistsHandler` to read `customer:{customerId}` from cache before calling `ICustomerRepository.ExistsAsync`.
- [x] 3.2 On cache miss, query PostgreSQL through the repository and store the boolean result in Redis with the configured TTL.
- [x] 3.3 Preserve the current API response contract for `GET /api/v1/customers/{customerId}/exists`.

## 4. Local Configuration

- [x] 4.1 Add Redis connection and cache TTL settings to `appsettings.json` and `appsettings.Development.json`.
- [x] 4.2 Add Redis placeholder values to `.env.example`.
- [x] 4.3 Extend `docker-compose.yml` with a Redis service and pass the Redis connection string to `customer-api`.

## 5. Tests

- [x] 5.1 Add unit tests verifying cache hit returns the cached value and does not query the repository.
- [x] 5.2 Add unit tests verifying cache miss queries the repository and writes the result with TTL.
- [x] 5.3 Add unit tests verifying Redis read and write failures still return the repository result.
- [x] 5.4 Add integration tests using Redis and PostgreSQL test containers to verify the endpoint creates `customer:{customerId}` with TTL after a cache miss.
- [x] 5.5 Run `dotnet test` and fix any regressions.

## 6. Documentation

- [x] 6.1 Document the customer exists Redis cache flow, key format, and TTL configuration in `README.md`.
- [x] 6.2 Document that future customer create, update, and delete flows must invalidate `customer:{customerId}` when they change existence semantics.
