## ADDED Requirements

### Requirement: Customer profile picture upload URL endpoint is exposed
The system SHALL expose a versioned endpoint for customer profile picture upload URL generation.

#### Scenario: Versioned route is available
- **WHEN** Swagger/OpenAPI is generated for API version 1
- **THEN** it includes `POST /api/v1/customers/{customerId}/profile-picture/upload-url`.

#### Scenario: Unauthorized caller is rejected
- **WHEN** a caller requests `POST /api/v1/customers/{customerId}/profile-picture/upload-url` without a valid access token
- **THEN** the API responds with an unauthorized error.

#### Scenario: Customer caller cannot request another customer's upload URL
- **WHEN** a customer role token requests `POST /api/v1/customers/{customerId}/profile-picture/upload-url`
- **THEN** the API responds with a forbidden error.
