## Context

`customer-service` owns customer balances and already issues JWT tokens with a `customer_id` claim for users in the `Customer` role. The existing API uses versioned customer routes, application handlers, FluentValidation, ProblemDetails-style error middleware, and EF Core persistence with banking details stored separately from the customer profile.

The new operation represents a customer-to-customer balance movement initiated by the authenticated sender. It must not let clients choose the sender in the payload, and it must not persist a debit without the matching credit.

## Goals / Non-Goals

**Goals:**
- Expose a protected balance update endpoint for authenticated customers.
- Resolve the sender from the JWT `customer_id` claim and receive only `receiverId` and `amount` from the body.
- Validate request shape, customer existence, self-transfer, and available balance before committing changes.
- Persist sender debit and receiver credit atomically in one database transaction.
- Add focused unit and integration coverage for success, failure, and rollback behavior.

**Non-Goals:**
- Create transfer history or transfer status ownership inside `customer-service`.
- Implement `transfer-service` orchestration.
- Add external messaging for this operation unless a later transfer workflow requires it.
- Allow Admin or Service users to initiate this customer transfer endpoint.

## Decisions

1. Use `POST /api/v1/customers/update-balance`.

   The operation changes two resources and represents a command, not a partial update of one customer representation. Keeping it under `/customers` follows the existing controller route style while making the command name explicit.

2. Derive sender identity from the authenticated JWT.

   The controller will require `Authorize(Roles = nameof(UserRole.Customer))`, read the existing `customer_id` claim, and pass that id to the application handler. The request body will not contain sender id, preventing clients from debiting another customer.

3. Keep balance rules in the domain and orchestration in the application layer.

   `BankingDetails` or `Customer` should expose explicit debit/credit behavior that rejects negative amounts and insufficient funds. The application handler should coordinate loading both customers, invoking domain methods, and returning a response. Infrastructure should only implement persistence details.

4. Add an application-level transaction abstraction.

   The application layer needs to express "load both customers, mutate both, commit once" without referencing EF Core. Introduce a small abstraction such as `ICustomerBalanceTransferRepository` or `IUnitOfWork` owned by Application and implemented in Infrastructure. The implementation should use an EF Core transaction and a single `SaveChangesAsync`.

5. Lock or order balance updates consistently.

   When loading both customer rows for update, the implementation should use the strongest practical consistency available in the current EF Core/PostgreSQL setup. At minimum, load both customers in a deterministic customer-id order inside the transaction to reduce deadlock risk. If row-level locking is added, keep provider-specific SQL in Infrastructure.

6. Return only sender-relevant balance data.

   The response should include sender id, receiver id, amount, and sender balance after the operation. It should not expose the receiver's resulting balance to the sender.

## Risks / Trade-offs

- Concurrent transfers may overspend if balance checks are performed on stale data -> keep reads and writes inside one transaction and add row-level locking or an equivalent concurrency control in Infrastructure.
- Provider-specific row locking can reduce portability -> isolate provider-specific SQL behind Infrastructure contracts.
- A command endpoint named `update-balance` is less REST-pure than a resource route -> it matches the requested endpoint name and clearly communicates the operation.
- Integration rollback tests can be slower because they need a real database -> keep the suite focused on one success path and the important failure/rollback paths.
