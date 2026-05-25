## ADDED Requirements

### Requirement: Customer details endpoint returns profile and banking details
The system SHALL expose a versioned endpoint to retrieve a customer by id and return customer profile information together with banking details owned by `customer-service`.

#### Scenario: Customer details are found
- **WHEN** a client sends `GET /api/v1/customers/{customerId}` for an existing customer
- **THEN** the API responds with `200 OK` and a body containing id, name, email, address, profile picture URL when available, agency, and checking account number

#### Scenario: Customer details are not found
- **WHEN** a client sends `GET /api/v1/customers/{customerId}` for a customer id that does not exist
- **THEN** the API responds with a problem details `404 Not Found` response

### Requirement: Customer partial update endpoint accepts supported fields
The system SHALL expose a versioned endpoint to partially update a customer by id using any non-empty subset of `name`, `email`, `address`, and `bankingDetails`.

#### Scenario: Customer profile fields are partially updated
- **WHEN** a client sends `PATCH /api/v1/customers/{customerId}` with one or more of `name`, `email`, or `address`
- **THEN** the API persists only the supplied fields and responds with an update status

#### Scenario: Customer banking details are partially updated
- **WHEN** a client sends `PATCH /api/v1/customers/{customerId}` with `bankingDetails.agency` or `bankingDetails.checkingAccountNumber`
- **THEN** the API persists only the supplied banking fields and responds with an update status

#### Scenario: Partial update rejects empty payload
- **WHEN** a client sends `PATCH /api/v1/customers/{customerId}` without any supported fields
- **THEN** the API responds with a validation problem response

#### Scenario: Partial update rejects invalid supplied values
- **WHEN** a client sends `PATCH /api/v1/customers/{customerId}` with blank `name`, `email`, `address`, `bankingDetails.agency`, invalid `email`, or blank `bankingDetails.checkingAccountNumber`
- **THEN** the API responds with a validation problem response and does not persist the update

#### Scenario: Partial update target is not found
- **WHEN** a client sends `PATCH /api/v1/customers/{customerId}` for a customer id that does not exist
- **THEN** the API responds with a problem details `404 Not Found` response
