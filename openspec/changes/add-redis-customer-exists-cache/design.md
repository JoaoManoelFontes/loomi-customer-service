## Context

`GET /api/v1/customers/{customerId}/exists` currently delegates to `CustomerExistsHandler`, which calls `ICustomerRepository.ExistsAsync` and checks PostgreSQL on every request. This endpoint is expected to be used by other services before operations such as transfers, so repeated checks for the same customer id should avoid unnecessary database reads.

The project already keeps infrastructure concerns behind application abstractions. Redis access must remain outside controllers and outside the domain model.

## Goals / Non-Goals

**Goals:**
- Add a Redis-backed cache path for customer existence checks.
- Preserve the existing endpoint contract: HTTP 200 with a boolean body for valid ids.
- Use `customer:{id}` as the cache key.
- Store cache entries with a configurable TTL.
- Keep PostgreSQL as the source of truth on cache miss.
- Add focused unit tests and integration tests for hit, miss, write, TTL, and Redis fallback behavior.

**Non-Goals:**
- Do not cache full customer details for all customer endpoints in this change.
- Do not implement cache invalidation for customer update flows unless required by the exists endpoint behavior.
- Do not introduce distributed locking or cache stampede protection.
- Do not change authentication, authorization, or route shape.

## Decisions

1. Add an application-level cache abstraction for customer existence.

   The exists use case should depend on an application abstraction such as `ICustomerExistenceCache` or an existing generic cache port if one is already present. Infrastructure will provide the Redis implementation.

   Alternative considered: inject `IDatabase` from StackExchange.Redis into the handler. That would leak infrastructure into the application layer and weaken Clean Architecture boundaries.

2. Cache the existence result, not the full customer entity.

   The first consumer is an endpoint that returns only `true` or `false`, so the cached payload should be a small entry containing the existence boolean and enough metadata for serialization if needed. This avoids stale profile data concerns and keeps the change narrow.

   Alternative considered: cache the complete customer object at `customer:{id}` and derive existence from it. That creates unnecessary invalidation requirements for fields that the endpoint does not return.

3. Use `customer:{id}` as the key for the exists cache entry.

   This follows the requested key format. The implementation should normalize `id` using the `Guid` string format already used by the application to avoid duplicate keys for the same identifier.

   Alternative considered: use `customers:{id}:exists` for clearer namespacing. The requested contract explicitly names `customer:{id}`, so this change should honor it.

4. Read-through flow in the handler.

   The handler should execute:
   - Build key `customer:{id}`.
   - Try Redis.
   - If a cache entry exists, return it.
   - If no cache entry exists, call PostgreSQL through `ICustomerRepository.ExistsAsync`.
   - Save the boolean result in Redis with TTL.
   - Return the boolean result.

5. Redis failures should not fail the endpoint.

   If Redis read or write fails, the handler should log the cache failure through an abstraction or allow the infrastructure cache service to degrade gracefully, then continue with PostgreSQL. The endpoint should only fail when the source-of-truth PostgreSQL path fails.

   Alternative considered: return a 5xx when Redis is unavailable. That would make an optional performance layer part of endpoint availability.

6. TTL should be configurable.

   Add configuration such as `Redis:CustomerExistsTtlSeconds` or `CustomerCache:ExistsTtlSeconds`, with a safe local default. Tests should verify that the configured TTL is passed to Redis writes.

## Risks / Trade-offs

- Stale negative cache after a customer is created -> Use a short TTL for existence entries and document that future create/update flows should invalidate `customer:{id}` when they are added.
- Stale positive cache after a customer is deleted -> Customer deletion is not currently in scope; future delete flows must remove `customer:{id}`.
- Redis outage could hide cache test issues -> Add unit tests for cache exceptions and integration tests with Redis through Testcontainers.
- The generic key `customer:{id}` may later conflict with full customer detail caching -> Keep the cached payload versioned or typed internally so future consumers can migrate deliberately.

## Migration Plan

1. Add Redis package and configuration if not already present.
2. Add the application cache abstraction and Redis implementation.
3. Register Redis connection and cache service in Infrastructure dependency injection.
4. Update `CustomerExistsHandler` to use read-through caching.
5. Extend Docker Compose and `.env.example` with Redis.
6. Add unit and integration tests.
7. Rollback by removing the cache dependency from the handler and leaving the repository-only path.

## Open Questions

- What TTL should be used for production? The implementation should start with a configurable default, for example 300 seconds, until operational requirements are clearer.
