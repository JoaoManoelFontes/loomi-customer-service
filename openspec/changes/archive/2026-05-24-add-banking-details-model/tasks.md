## 1. Domain Model

- [x] 1.1 Add `BankingDetails` to the Domain layer with agency, checking account number, and balance invariants.
- [x] 1.2 Update `Customer` so each customer owns one `BankingDetails` instance instead of loose banking fields.

## 2. Persistence

- [x] 2.1 Add EF Core configuration for the required one-to-one customer banking details relationship.
- [x] 2.2 Update `CustomerDbContext` if needed so banking details are part of the model.
- [x] 2.3 Generate an EF Core migration for the banking details table and relationship.

## 3. Tests and Verification

- [x] 3.1 Add focused unit tests for banking details creation and invalid values.
- [x] 3.2 Update existing customer tests affected by the banking details relationship.
- [x] 3.3 Run build and tests to verify the change.
