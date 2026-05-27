## 1. Application Layer

- [x] 1.1 Add profile picture upload URL request, response, validator, and handler.
- [x] 1.2 Add an application storage abstraction for creating profile picture upload URLs.
- [x] 1.3 Add unit tests for metadata validation and handler behavior.

## 2. Infrastructure Layer

- [x] 2.1 Add Azure Blob Storage options and implementation for signed upload URL generation.
- [x] 2.2 Add local fallback storage implementation for missing Azure configuration.
- [x] 2.3 Register the appropriate storage implementation in Infrastructure dependency injection.

## 3. API Layer

- [x] 3.1 Add `POST /api/v1/customers/{customerId}/profile-picture/upload-url`.
- [x] 3.2 Ensure Swagger documents the versioned endpoint and auth/error responses.
- [x] 3.3 Add integration tests for success, auth, not found, and validation scenarios.

## 4. Configuration and Verification

- [x] 4.1 Update appsettings, `.env.example`, and README with Azure Blob Storage upload URL configuration.
- [x] 4.2 Run `dotnet build` and `dotnet test`.
