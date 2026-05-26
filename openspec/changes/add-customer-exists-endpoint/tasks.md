## 1. Application Contract

- [x] 1.1 Add an `ExistsAsync(Guid id, CancellationToken cancellationToken = default)` method to the customer repository abstraction.
- [x] 1.2 Add an application query/use case for checking customer existence by id.
- [x] 1.3 Ensure the query/use case returns only a boolean result and does not expose customer details.

## 2. Infrastructure

- [x] 2.1 Implement the repository existence check using an efficient database query.
- [x] 2.2 Ensure the existence query does not load the full customer entity.

## 3. API Endpoint

- [x] 3.1 Add `GET /api/v1/customers/{customerId}/exists` to the versioned customers controller.
- [x] 3.2 Return HTTP 200 with `true` when the customer exists.
- [x] 3.3 Return HTTP 200 with `false` when the customer does not exist.
- [x] 3.4 Preserve normal client-error behavior for ids that cannot be parsed as a customer identifier.

## 4. Tests

- [x] 4.1 Add unit tests for the application existence query/use case.
- [x] 4.2 Add repository or integration coverage proving existing customers return `true`.
- [x] 4.3 Add repository or integration coverage proving missing customers return `false`.
- [x] 4.4 Add API integration coverage for the versioned endpoint response status and boolean body.

## 5. Verification

- [x] 5.1 Run `dotnet build`.
- [x] 5.2 Run `dotnet test`.
- [x] 5.3 Confirm Swagger/OpenAPI includes the new versioned endpoint.
