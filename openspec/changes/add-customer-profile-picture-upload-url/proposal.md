## Why

Customer profile pictures must be stored outside the relational database, and the frontend needs a secure way to upload image files directly to Blob Storage without routing file bytes through `customer-service`.

This change prepares the Azure Blob Storage integration now, while allowing the project to keep building and running locally before real Azure credentials are available.

## What Changes

- Add an endpoint to request a temporary upload URL for a customer profile picture:
  - `POST /api/v1/customers/{customerId}/profile-picture/upload-url`
- Add request validation for image metadata such as file name, content type, and file size.
- Add an application use case that generates a storage object name and delegates signed URL creation to a storage abstraction.
- Add an Azure Blob Storage implementation that can generate SAS upload URLs when credentials are configured.
- Add a local/no-op storage implementation used when Azure configuration is missing, so local development and tests do not break.
- Add response metadata needed by the frontend to upload the image and later reference the blob.

## Capabilities

### New Capabilities
- `customer-profile-picture-storage`: Customer profile picture upload URL generation and Blob Storage integration.

### Modified Capabilities
- `customers-api-foundation`: Adds the versioned customer profile picture upload URL endpoint to the existing customer API surface.

## Impact

- Affects API controllers, request/response contracts, validation, Swagger output, application command/query handlers, storage abstractions, infrastructure dependency injection, and app configuration.
- Uses the existing Azure Blob Storage package already present in the Infrastructure project.
- Does not require real Azure credentials for local execution; missing storage configuration returns a deterministic local placeholder URL.
