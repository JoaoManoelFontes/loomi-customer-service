## Context

`customer-service` already exposes versioned customer endpoints and uses the Application layer for use cases plus Infrastructure for external implementations. Profile image URLs can be persisted through the existing customer update flow, but the service does not yet provide a way for the frontend to upload image bytes to object storage.

Azure credentials are not available yet, so the implementation must keep local development, tests, and Swagger usable without a real storage account.

## Goals / Non-Goals

**Goals:**
- Generate a temporary upload URL for `POST /api/v1/customers/{customerId}/profile-picture/upload-url`.
- Validate image metadata before generating a URL.
- Keep Azure SDK usage inside Infrastructure behind an Application abstraction.
- Provide a local fallback implementation when Azure Blob Storage is not configured.
- Return enough metadata for the frontend to upload the file and later submit the final blob URL through the existing profile update flow.

**Non-Goals:**
- Uploading image bytes through the API.
- Persisting the profile picture URL as part of upload URL generation.
- Creating Azure resources or requiring live Azure credentials in tests.
- Implementing malware scanning, CDN integration, or image resizing.

## Decisions

1. Add `IProfilePictureStorageService` in Application.
   - Rationale: signed URL generation is a storage concern, but the use case belongs to Application.
   - Alternative considered: put Azure SDK calls directly in the controller. Rejected because it would couple HTTP to Infrastructure and make tests harder.

2. Use a dedicated `CreateProfilePictureUploadUrlHandler`.
   - Rationale: the controller remains thin and validation stays out of HTTP actions.
   - Alternative considered: fold this into `UpdateCustomerHandler`. Rejected because upload URL generation does not mutate the customer record.

3. Use SAS write/create permissions for Azure Blob Storage when configured.
   - Rationale: the frontend only needs permission to create or overwrite the generated blob for a short time.
   - Alternative considered: proxy file upload through the API. Rejected because it increases service bandwidth and does not match the requested pre-signed URL flow.

4. Register a local fallback storage service when Azure configuration is incomplete.
   - Rationale: the project must not break before real credentials exist.
   - Alternative considered: fail startup when storage config is missing. Rejected because it blocks local development and unrelated tests.

5. Generate object names server-side under a customer-specific prefix.
   - Rationale: clients should not control blob paths, and customer IDs make cleanup/auditing easier.
   - Format: `customers/{customerId}/profile-pictures/{generatedId}{extension}`.

## Risks / Trade-offs

- Missing Azure permissions or invalid connection string -> fallback avoids local startup failures, but real environments must configure storage explicitly before production use.
- Client uploads a different payload than declared metadata -> SAS URL includes expected content type when possible, but content validation at storage level is limited.
- Upload URL is generated before the customer updates `ProfilePictureUrl` -> the final profile URL still depends on the existing profile update endpoint or a future confirmation endpoint.
- Public blob URL shape varies by account/container settings -> the response returns both upload URL and blob URL calculated by the storage service.
