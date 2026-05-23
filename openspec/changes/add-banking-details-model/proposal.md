## Why

Customer banking information is currently represented as loose fields on `Customer`, which mixes customer identity data with account-specific data. Introducing a dedicated `BankingDetails` model makes the domain clearer, keeps balance ownership inside `customer-service`, and prepares the codebase for future debit, credit, and banking-detail update flows.

## What Changes

- Add a `BankingDetails` domain model containing agency, checking account number, and balance.
- Associate each `Customer` with exactly one `BankingDetails` instance.
- Persist the relationship with EF Core using explicit Fluent API mapping and a database migration.
- Add focused tests for banking details creation and customer ownership behavior where useful.
- Keep transfer business flow out of scope; this change only models and persists customer-owned banking data.

## Capabilities

### New Capabilities
- `banking-details-model`: Customer-owned banking details model, one-to-one customer relationship, persistence mapping, migration, and domain tests.

### Modified Capabilities

## Impact

- `CustomerService.Domain`: new banking details entity/model and customer relationship changes.
- `CustomerService.Infrastructure`: DbContext, EF Core configuration, model snapshot, and migration.
- `CustomerService.UnitTests`: tests covering model invariants and customer relationship behavior.
- No public API contract changes are required in this change.
