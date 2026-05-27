## Why

Customer balance changes need a service-owned operation that can move funds from an authenticated sender to a receiver without allowing partial updates. This is needed before `transfer-service` can safely coordinate transfers while keeping balance ownership inside `customer-service`.

## What Changes

- Add a protected `update-balance` endpoint that identifies the sender from the customer JWT token.
- Accept `receiverId` and transfer `amount` in the request body.
- Validate authentication, customer linkage, receiver existence, positive amount, self-transfer, and sender available balance.
- Update sender and receiver balances atomically in a single database transaction.
- Return consistent ProblemDetails-style errors without leaking internal details.
- Add unit and integration tests for successful transfer, validation failures, authorization failures, not-found receivers, insufficient balance, and transaction rollback.

## Capabilities

### New Capabilities
- `customer-balance-transfer`: Customer-owned balance transfer endpoint, validations, transactional persistence, and tests.

### Modified Capabilities

## Impact

- API: new versioned customer balance update endpoint and request/response contracts.
- Application: new command, handler, validator, and repository/unit-of-work abstractions as needed.
- Domain: balance mutation rules may be added to the `Customer` aggregate/entity.
- Infrastructure: transactional EF Core implementation for updating both customer balances.
- Tests: unit tests for validation/application behavior and integration tests for HTTP/database transaction behavior.
