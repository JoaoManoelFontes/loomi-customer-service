## ADDED Requirements

### Requirement: Customer banking details model
The system SHALL model customer banking details as a dedicated domain model containing agency, checking account number, and balance.

#### Scenario: Banking details are created with valid data
- **WHEN** valid agency, checking account number, and non-negative balance values are provided
- **THEN** the system creates a banking details model with those values

#### Scenario: Banking details reject invalid data
- **WHEN** agency or checking account number is blank, or initial balance is negative
- **THEN** the system rejects the banking details model creation

### Requirement: Customer owns one banking details record
The system SHALL associate each customer with exactly one banking details record owned by `customer-service`.

#### Scenario: Customer is created with banking details
- **WHEN** a customer is created
- **THEN** the customer has one associated banking details record

### Requirement: Banking details persistence
The system SHALL persist banking details through EF Core using explicit Fluent API configuration and a database migration.

#### Scenario: Banking details relationship is mapped
- **WHEN** EF Core builds the customer model
- **THEN** the model includes a required one-to-one relationship between customer and banking details
