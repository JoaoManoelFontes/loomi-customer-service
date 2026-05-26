## ADDED Requirements

### Requirement: Customer exists uses Redis read-through cache
The system SHALL use Redis as a read-through cache for `GET /api/v1/customers/{customerId}/exists` before querying PostgreSQL.

#### Scenario: Existing cached customer result is returned
- **WHEN** a client sends `GET /api/v1/customers/{customerId}/exists` and Redis contains `customer:{customerId}` with an existence value
- **THEN** the API returns HTTP 200 with the cached boolean value
- **AND** PostgreSQL is not queried for that request

#### Scenario: Missing cache entry reads PostgreSQL
- **WHEN** a client sends `GET /api/v1/customers/{customerId}/exists` and Redis does not contain `customer:{customerId}`
- **THEN** the system queries PostgreSQL for the customer existence result
- **AND** the API returns HTTP 200 with the PostgreSQL result

#### Scenario: Cache miss stores result with TTL
- **WHEN** PostgreSQL returns an existence result after a Redis cache miss
- **THEN** the system stores the result in Redis using key `customer:{customerId}`
- **AND** the Redis entry has a configured TTL

### Requirement: Redis cache is optional for endpoint availability
The system SHALL preserve customer exists endpoint behavior when Redis is unavailable.

#### Scenario: Redis read fails
- **WHEN** Redis read fails while handling `GET /api/v1/customers/{customerId}/exists`
- **THEN** the system queries PostgreSQL for the customer existence result
- **AND** the API returns HTTP 200 with the PostgreSQL result

#### Scenario: Redis write fails after PostgreSQL lookup
- **WHEN** Redis write fails after PostgreSQL returns an existence result
- **THEN** the API still returns HTTP 200 with the PostgreSQL result

### Requirement: Redis configuration is explicit
The system SHALL configure Redis through application configuration and local development environment values.

#### Scenario: Redis connection is configured
- **WHEN** the API starts with Redis configuration present
- **THEN** the service container includes a Redis-backed customer cache implementation

#### Scenario: Docker Compose starts Redis
- **WHEN** a developer runs `docker compose up -d`
- **THEN** Docker Compose starts a Redis service reachable by the customer API

### Requirement: Customer exists cache is tested
The system SHALL include automated tests for the customer exists Redis cache behavior.

#### Scenario: Unit tests cover cache hit and miss flow
- **WHEN** the customer exists handler tests run
- **THEN** they verify cache hit returns without repository access
- **AND** cache miss queries the repository and writes Redis with TTL

#### Scenario: Unit tests cover Redis fallback
- **WHEN** the customer exists handler tests run
- **THEN** they verify Redis read or write failures do not prevent returning the PostgreSQL result

#### Scenario: Integration tests cover Redis and PostgreSQL behavior
- **WHEN** integration tests run with PostgreSQL and Redis test containers
- **THEN** they verify the endpoint returns the expected boolean value and creates the `customer:{customerId}` Redis entry with TTL after a cache miss
