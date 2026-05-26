## Context

The customer API already owns customer data and exposes versioned HTTP routes. The transfer service and other future clients need to validate a customer id without retrieving full customer details or coupling to customer table structure.

## Goals / Non-Goals

**Goals:**

- Add a lightweight versioned endpoint that answers whether a customer exists in the database.
- Keep the response intentionally minimal: HTTP 200 with a boolean body.
- Route the check through Application and Infrastructure abstractions instead of querying persistence from the API layer.
- Make the existence lookup efficient and testable.

**Non-Goals:**

- Do not expose customer details from this endpoint.
- Do not create or update customers.
- Do not add transfer-service integration in this change.
- Do not add cache behavior unless implementation already has a safe shared pattern for existence checks.

## Decisions

- Use `GET /api/v1/customers/{customerId}/exists` as the route.
  - Rationale: it keeps the route under the customer resource and makes the operation explicit.
  - Alternative considered: `HEAD /api/v1/customers/{customerId}`. This was rejected because the requested contract is a boolean response body and HEAD cannot return one.

- Return `200 OK` with a boolean body for well-formed ids.
  - Rationale: clients can distinguish an invalid id format from a valid id that is absent, while avoiding exceptions for normal negative lookups.
  - Alternative considered: `404 Not Found` for absent customers. This was rejected because the requested behavior is a boolean result.

- Add an application-level query/use case, backed by a repository `ExistsAsync` method.
  - Rationale: existence is a business-facing read operation and should not require loading the full aggregate.
  - Alternative considered: reuse `GetByIdAsync` and check null. This is simpler but less efficient and can accidentally load unnecessary data.

- Keep persistence-specific implementation inside Infrastructure.
  - Rationale: the API and Application layers must not depend directly on EF Core or database details.

## Risks / Trade-offs

- Returning `false` for absent customers can hide whether the caller is authorized to know about a customer id. Mitigation: apply the existing authorization conventions for customer read endpoints when authentication/authorization is enabled.
- A boolean endpoint can be used for id enumeration if exposed publicly. Mitigation: keep it under the same security boundary as other customer endpoints and avoid logging sensitive identifiers unnecessarily.
- If repository tests only cover `GetByIdAsync`, the new optimized path can drift. Mitigation: add focused unit and integration tests for existing and missing customers.
