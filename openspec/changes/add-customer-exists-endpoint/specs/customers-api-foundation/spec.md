## ADDED Requirements

### Requirement: Customer existence endpoint is exposed
The system SHALL expose a versioned endpoint that checks whether a well-formed customer id represents a persisted customer.

#### Scenario: Existing customer returns true
- **WHEN** a client sends `GET /api/v1/customers/{customerId}/exists` with a well-formed id that exists in the customer database
- **THEN** the API responds with HTTP 200
- **AND** the response body is the boolean value `true`

#### Scenario: Missing customer returns false
- **WHEN** a client sends `GET /api/v1/customers/{customerId}/exists` with a well-formed id that does not exist in the customer database
- **THEN** the API responds with HTTP 200
- **AND** the response body is the boolean value `false`

#### Scenario: Invalid customer id is rejected
- **WHEN** a client sends `GET /api/v1/customers/{customerId}/exists` with an id that cannot be parsed as a customer identifier
- **THEN** the API responds with a client error status
- **AND** the response does not report the id as an absent valid customer

### Requirement: Customer existence check avoids loading customer details
The system SHALL check customer existence through a persistence operation that does not require loading the full customer record.

#### Scenario: Existence check uses repository abstraction
- **WHEN** the API handles `GET /api/v1/customers/{customerId}/exists`
- **THEN** the API delegates to the application layer
- **AND** the application layer uses a repository existence operation instead of exposing infrastructure details to the controller
