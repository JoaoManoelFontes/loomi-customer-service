## Why

The customer existence check is a high-frequency read path used by service-to-service flows, especially future transfer validation. Adding Redis caching reduces repeated PostgreSQL lookups for the same customer id while keeping the endpoint behavior unchanged.

## What Changes

- Add a Redis-backed customer lookup cache capability.
- Apply the cache first to `GET /api/v1/customers/{customerId}/exists`.
- Use the key format `customer:{id}` for cached customer lookup data.
- On cache hit, return the existence result without querying PostgreSQL.
- On cache miss, query PostgreSQL, store the result in Redis with a configured TTL, and return the result.
- Add unit and integration coverage for cache hit, cache miss, TTL write, and fallback behavior.

## Capabilities

### New Capabilities
- `customer-redis-cache`: Defines Redis cache behavior for customer read paths, starting with the customer exists endpoint.

### Modified Capabilities
- None.

## Impact

- Affects `CustomerService.Application` customer exists use case and cache abstractions.
- Affects `CustomerService.Infrastructure` Redis configuration and cache implementation.
- Affects `CustomerService.Api` configuration and dependency injection composition.
- Affects unit and integration tests around `GET /api/v1/customers/{customerId}/exists`.
- Requires local Redis configuration through appsettings and Docker Compose environment values.
