## ADDED Requirements

### Requirement: Customer balance update endpoint is exposed
The system SHALL expose a versioned `POST /api/v1/customers/update-balance` endpoint that allows an authenticated customer to transfer balance to another customer.

#### Scenario: Successful balance update returns sender balance
- **WHEN** an authenticated Customer submits a valid receiver id and positive amount
- **THEN** the API responds with success and returns the sender id, receiver id, transferred amount, and sender balance after the operation

#### Scenario: Endpoint does not accept sender id in body
- **WHEN** a client submits a balance update request
- **THEN** the sender customer id is resolved from the authenticated token `customer_id` claim and not from the request body

#### Scenario: Swagger includes endpoint
- **WHEN** Swagger is opened in development
- **THEN** the OpenAPI document includes `POST /api/v1/customers/update-balance` with bearer authentication metadata

### Requirement: Balance update endpoint requires customer authentication
The system SHALL require a valid JWT bearer token with the `Customer` role and a valid `customer_id` claim before executing a balance update.

#### Scenario: Missing token is unauthorized
- **WHEN** an anonymous client calls `POST /api/v1/customers/update-balance`
- **THEN** the API rejects the request as unauthorized

#### Scenario: Non-customer role is forbidden
- **WHEN** an authenticated Admin or Service user calls `POST /api/v1/customers/update-balance`
- **THEN** the API rejects the request as forbidden

#### Scenario: Missing customer claim is forbidden
- **WHEN** an authenticated Customer token does not contain a valid `customer_id` claim
- **THEN** the API rejects the request as forbidden

### Requirement: Balance update request is validated
The system SHALL validate balance update input before loading or mutating balances.

#### Scenario: Receiver id is required
- **WHEN** a balance update request omits `receiverId` or sends an empty GUID
- **THEN** the API returns a validation error

#### Scenario: Amount must be positive
- **WHEN** a balance update request sends an amount less than or equal to zero
- **THEN** the API returns a validation error

#### Scenario: Sender cannot transfer to self
- **WHEN** the authenticated sender customer id equals the request `receiverId`
- **THEN** the API rejects the request with a validation or business-rule error

### Requirement: Balance update verifies customers and funds
The system SHALL verify that both sender and receiver customers exist and that the sender has enough available balance before committing changes.

#### Scenario: Sender customer is not found
- **WHEN** the token customer id does not match an existing customer
- **THEN** the API returns a not-found error and no balance is changed

#### Scenario: Receiver customer is not found
- **WHEN** the request `receiverId` does not match an existing customer
- **THEN** the API returns a not-found error and no balance is changed

#### Scenario: Sender has insufficient balance
- **WHEN** the sender balance is lower than the requested amount
- **THEN** the API returns a business-rule error and no balance is changed

### Requirement: Balance update is atomic
The system SHALL debit the sender balance and credit the receiver balance inside one database transaction.

#### Scenario: Both balances are updated in one transaction
- **WHEN** a valid balance update is processed
- **THEN** the sender balance is decreased and the receiver balance is increased by the same amount in a single committed transaction

#### Scenario: Failure rolls back all balance changes
- **WHEN** any validation, business-rule, persistence, or cancellation failure occurs before commit
- **THEN** neither the sender nor the receiver balance is persisted with a partial change

#### Scenario: Concurrent updates do not create negative balance
- **WHEN** concurrent balance update requests would together exceed the sender available balance
- **THEN** the system allows only updates that can be covered by the committed sender balance and rejects the rest without persisting a negative balance

### Requirement: Balance update errors are consistent and safe
The system SHALL return ProblemDetails-style errors for balance update failures without exposing stack traces or internal persistence details.

#### Scenario: Validation errors use validation problem details
- **WHEN** the request body fails balance update validation
- **THEN** the API returns a validation ProblemDetails response

#### Scenario: Business errors use problem details
- **WHEN** the operation fails because of self-transfer, not-found customer, or insufficient balance
- **THEN** the API returns a ProblemDetails response with an appropriate HTTP status code

#### Scenario: Internal errors do not leak implementation details
- **WHEN** an unexpected exception occurs during balance update processing
- **THEN** the API returns a safe ProblemDetails response without stack trace content

### Requirement: Balance update tests cover behavior
The system SHALL include unit and integration tests for balance update behavior.

#### Scenario: Unit tests cover domain balance rules
- **WHEN** the unit test suite runs
- **THEN** it verifies debit, credit, invalid amount, and insufficient balance domain behavior

#### Scenario: Unit tests cover application handler paths
- **WHEN** the unit test suite runs
- **THEN** it verifies successful transfer, missing sender, missing receiver, self-transfer, insufficient balance, validation failures, and transaction rollback signals

#### Scenario: Integration tests cover HTTP and persistence
- **WHEN** the integration test suite runs
- **THEN** it verifies authenticated success, unauthorized access, forbidden access, validation failure, receiver not found, insufficient balance, and persisted atomic balance changes
