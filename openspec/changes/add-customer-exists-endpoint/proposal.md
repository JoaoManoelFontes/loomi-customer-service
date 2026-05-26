## Why

Other services need a lightweight way to confirm whether a customer identifier belongs to a persisted customer before starting workflows that depend on customer ownership. This avoids fetching the full customer record when only existence validation is needed.

## What Changes

- Add a versioned HTTP endpoint to check customer existence by id.
- Return HTTP 200 with a boolean response body: `true` when the customer exists, `false` when the customer does not exist.
- Keep the endpoint read-only and scoped to database-backed customer validity.
- Do not return customer details or business data from this endpoint.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `customers-api-foundation`: add the customer existence endpoint behavior to the customer API contract.

## Impact

- Adds a new route under the existing customer API surface: `GET /api/v1/customers/{customerId}/exists`.
- Requires an application query or service path that checks persisted customer existence.
- Requires repository support for efficient existence checks.
- Adds unit and integration tests covering true and false existence results.
