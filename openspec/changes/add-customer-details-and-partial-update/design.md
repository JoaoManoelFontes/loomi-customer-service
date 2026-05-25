## Context

The API already exposes versioned customer routes and uses application handlers with FluentValidation. The domain model includes `Customer` and owned `BankingDetails`, and persistence is backed by EF Core with PostgreSQL.

## Goals / Non-Goals

**Goals:**
- Add a read endpoint for customer details including agency and checking account number.
- Add a partial update endpoint that accepts any non-empty subset of `name`, `email`, `address`, and `bankingDetails`.
- Keep validation outside controllers and keep EF Core access behind an application abstraction.
- Cover validation and handler behavior with focused tests.

**Non-Goals:**
- Do not implement transfer flows, debit/credit operations, or balance mutation.
- Do not add caching or messaging behavior in this change.
- Do not change authentication or authorization policy beyond matching existing customer route conventions.

## Decisions

- Use application handlers instead of introducing a new mediator pipeline.
  Rationale: the project already uses explicit handlers (`CreateUserHandler`, `LoginHandler`), so this keeps the change consistent and small. Alternative considered: MediatR requests; deferred until the project consistently adopts it.

- Add `ICustomerRepository` in the application layer and implement it in infrastructure.
  Rationale: customer read/update use cases should not reference EF Core directly. Alternative considered: reusing `IUserRepository`; rejected because customer retrieval is a separate use case from authentication/user creation.

- Model the patch request with nullable properties and a nested nullable `bankingDetails` object.
  Rationale: null means "not supplied", while supplied blank strings are validation failures. Alternative considered: JSON Patch; rejected as too broad for this challenge and less explicit for banking details.

- Return a compact update status response from PATCH.
  Rationale: the user requested update status, while GET remains the source for full customer details after mutation.

## Risks / Trade-offs

- Partial updates must distinguish omitted values from invalid blank values -> validators use null checks before applying per-field rules.
- Banking details currently has private setters and no update method -> add a small domain method to keep invariants centralized.
- Integration tests may require database setup -> use unit tests for handlers/validators and keep endpoint wiring covered by controller-level behavior where possible.
