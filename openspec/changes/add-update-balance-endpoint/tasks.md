## 1. Domain and Application Contracts

- [x] 1.1 Add domain balance mutation methods for debit and credit with positive amount and insufficient balance rules.
- [x] 1.2 Add application request/response contracts for `UpdateBalanceRequest` and `UpdateBalanceResponse`.
- [x] 1.3 Add a FluentValidation validator for `UpdateBalanceRequest`.
- [x] 1.4 Add application exceptions for insufficient balance and invalid self-transfer if existing exceptions do not cover them.
- [x] 1.5 Add an application persistence abstraction for atomic balance transfer or unit-of-work transaction behavior.

## 2. Application Handler

- [x] 2.1 Add `UpdateBalanceHandler` that receives sender customer id from the caller and request data from the body.
- [x] 2.2 Validate sender and receiver are different before mutating balances.
- [x] 2.3 Load sender and receiver customers with banking details through the application abstraction.
- [x] 2.4 Return not-found errors when sender or receiver customer does not exist.
- [x] 2.5 Debit sender, credit receiver, and commit both balance updates through one atomic persistence operation.
- [x] 2.6 Return sender id, receiver id, amount, and sender balance after commit.

## 3. Infrastructure Persistence

- [x] 3.1 Implement the balance transfer persistence abstraction in Infrastructure using `CustomerDbContext`.
- [x] 3.2 Execute sender and receiver balance updates inside one EF Core database transaction.
- [x] 3.3 Load or update rows in deterministic customer-id order to reduce deadlock risk.
- [x] 3.4 Use a single commit path so failures before commit roll back both balance changes.
- [x] 3.5 Register the new persistence abstraction and handler dependencies in dependency injection.

## 4. API Endpoint

- [x] 4.1 Add `POST /api/v1/customers/update-balance` to the versioned customer controller.
- [x] 4.2 Protect the endpoint with Customer-role authorization.
- [x] 4.3 Resolve sender customer id from the JWT `customer_id` claim and reject missing or invalid claims as forbidden.
- [x] 4.4 Bind `receiverId` and `amount` from the request body only.
- [x] 4.5 Document response and ProblemDetails status codes with controller attributes so Swagger is accurate.

## 5. Error Handling

- [x] 5.1 Map invalid self-transfer to a safe ProblemDetails response.
- [x] 5.2 Map insufficient balance to a safe ProblemDetails response.
- [x] 5.3 Ensure validation errors, not-found customers, unauthorized, forbidden, and unexpected errors remain consistent with existing middleware.
- [x] 5.4 Confirm no stack traces or persistence implementation details are exposed to clients.

## 6. Tests

- [x] 6.1 Add unit tests for domain debit and credit success paths.
- [x] 6.2 Add unit tests for domain invalid amount and insufficient balance failures.
- [x] 6.3 Add unit tests for `UpdateBalanceRequestValidator`.
- [x] 6.4 Add unit tests for `UpdateBalanceHandler` success, sender not found, receiver not found, self-transfer, insufficient balance, and rollback/commit behavior.
- [x] 6.5 Add integration tests for authenticated HTTP success and persisted sender/receiver balance changes.
- [x] 6.6 Add integration tests for unauthorized, forbidden, validation failure, receiver not found, and insufficient balance responses.
- [x] 6.7 Add an integration test proving a failed update does not persist a partial sender debit or receiver credit.

## 7. Verification

- [x] 7.1 Run `dotnet test`.
- [x] 7.2 Run `dotnet build`.
- [ ] 7.3 Start the API locally and verify Swagger lists `POST /api/v1/customers/update-balance`.
