## 1. Application Contracts

- [x] 1.1 Add customer repository abstraction for loading and updating customers with banking details
- [x] 1.2 Add customer details response contracts and query handler
- [x] 1.3 Add partial update request/status contracts, validator, and handler

## 2. Domain and Infrastructure

- [x] 2.1 Add domain methods needed to update customer profile and banking detail fields without bypassing invariants
- [x] 2.2 Implement EF Core customer repository and register it in dependency injection

## 3. API

- [x] 3.1 Add `GET /api/v1/customers/{customerId}` endpoint returning customer details
- [x] 3.2 Add `PATCH /api/v1/customers/{customerId}` endpoint returning update status and problem details responses

## 4. Tests and Verification

- [x] 4.1 Add unit tests for partial update validation
- [x] 4.2 Add unit tests for customer details and partial update handlers
- [x] 4.3 Run OpenSpec validation, build, and test suite
