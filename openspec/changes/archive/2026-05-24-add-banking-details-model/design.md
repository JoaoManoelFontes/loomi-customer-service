## Context

`Customer` currently carries banking fields directly. That worked for the initial scaffold, but banking data has different semantics from identity/profile data: agency and checking account identify the account, and balance is a customer-service-owned monetary value that future transfer coordination will depend on.

The repository already uses Clean Architecture and EF Core mapping classes, so this change should stay within the existing Domain and Infrastructure boundaries.

## Goals / Non-Goals

**Goals:**
- Model banking details explicitly in the Domain layer.
- Keep each customer associated with exactly one banking details record.
- Preserve balance ownership in `customer-service`.
- Persist the model through EF Core Fluent API and a migration.
- Add narrow tests for domain behavior and invariants.

**Non-Goals:**
- Implement debit, credit, transfer orchestration, or event publication.
- Change public HTTP response/request contracts unless required by compilation.
- Add external dependencies.

## Decisions

- Use a dedicated `BankingDetails` domain entity instead of keeping flattened fields on `Customer`.
  - Rationale: agency, checking account, and balance change together as account state and deserve a named model. This avoids a bloated `Customer` entity and creates a clear place for future account rules.
  - Alternative considered: EF owned type. Rejected for now because the requirement asks for a model and relationship, and a separate table gives clearer persistence boundaries for future account-specific operations.

- Use a required one-to-one relationship from `Customer` to `BankingDetails`.
  - Rationale: the domain states each customer has banking details, and EF can enforce this with a unique foreign key.
  - Alternative considered: one-to-many account records. Rejected because the current requirement is exactly one banking details record per customer.

- Keep balance as `decimal` with explicit precision in EF mapping.
  - Rationale: monetary values require decimal precision; floating point types are inappropriate for account balances.
  - Alternative considered: a `Money` value object. Deferred to avoid premature abstraction until currency/multi-currency requirements exist.

- Use domain methods/factory-style construction to keep invariants near the model.
  - Rationale: basic validation for agency, checking account, and negative initial balance belongs in Domain, not EF or controllers.
  - Alternative considered: public setters only. Rejected because it would allow invalid state from application code.

## Risks / Trade-offs

- Existing rows without banking details can violate a required relationship after deployment. -> The migration should create the table without attempting to backfill existing customer rows unless seed data exists; local development can reset or add records through future use cases.
- Moving fields out of `Customer` may affect code that still reads `Agency`, `AccountNumber`, or `Balance` directly. -> Update usages and tests in the same change.
- Decimal precision must match banking expectations. -> Use a conservative `decimal(18,2)` mapping for the current challenge scope and document that finer precision can be revisited if requirements change.

## Migration Plan

1. Add the domain model and customer navigation.
2. Add EF Core configuration for the one-to-one relationship.
3. Generate a migration adding the `BankingDetails` table and foreign key/index.
4. Run build and tests.
5. Rollback by removing the migration and reverting the domain/model mapping changes before deployment.

## Open Questions

- None for this scope. Currency and account-number normalization can be addressed when debit/credit flows are designed.
