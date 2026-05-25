## Why

The customer API foundation currently exposes creation support but does not yet provide the challenge's required customer read and partial update endpoints. The service needs a clean application-layer path to retrieve customer details, including banking details, and to update only the fields provided by a client.

## What Changes

- Add `GET /api/v1/customers/{customerId}` to return customer profile data and banking details.
- Add `PATCH /api/v1/customers/{customerId}` to partially update a subset of `name`, `email`, `address`, and `bankingDetails`.
- Add request/response contracts, validators, application handlers, repository support, and API tests for the new endpoints.
- Keep customer balance owned by `customer-service`; the read response may expose balance if already part of the banking details model, but transfer flows remain out of scope.

## Capabilities

### New Capabilities

### Modified Capabilities
- `customers-api-foundation`: Adds customer details retrieval and partial update behavior to the versioned customer API.

## Impact

- Affected API surface: `CustomerService.Api.Controllers.V1` and Swagger metadata.
- Affected application layer: customer query/update contracts, validators, handlers, and repository abstractions.
- Affected infrastructure: EF Core customer repository implementation loading banking details.
- Affected tests: unit tests for validation/handlers and integration tests for endpoint routing and responses.
