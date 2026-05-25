## ADDED Requirements

### Requirement: Customer model captures profile data
The system SHALL provide a Customer domain model with name, email, address, and profile picture URL profile data.

#### Scenario: Customer is created with required profile data
- **WHEN** a Customer is created with valid name, email, and address
- **THEN** the Customer exposes the provided name, email, and address

#### Scenario: Customer may be created without a profile picture
- **WHEN** a Customer is created without a profile picture URL
- **THEN** the Customer remains valid and exposes no profile picture URL

### Requirement: Customer validates required profile fields
The system SHALL reject invalid Customer values for required profile data.

#### Scenario: Missing name is rejected
- **WHEN** a Customer is created with an empty name
- **THEN** the operation fails with a domain validation error

#### Scenario: Missing email is rejected
- **WHEN** a Customer is created with an empty email
- **THEN** the operation fails with a domain validation error

#### Scenario: Missing address is rejected
- **WHEN** a Customer is created with an empty address
- **THEN** the operation fails with a domain validation error

### Requirement: Customer supports profile updates
The system SHALL allow the Customer domain model to update profile data while preserving validation invariants.

#### Scenario: Profile data is updated
- **WHEN** a Customer updates name, email, and address with valid values
- **THEN** the Customer exposes the updated profile data

#### Scenario: Profile picture URL is updated
- **WHEN** a Customer updates the profile picture URL with a valid URL value
- **THEN** the Customer exposes the updated profile picture URL

#### Scenario: Invalid update is rejected
- **WHEN** a Customer updates required profile data with an empty value
- **THEN** the operation fails with a domain validation error
