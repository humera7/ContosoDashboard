# Research: Document Upload and Management

## Decision

The feature will use a secure local file store outside `wwwroot` with a storage abstraction named `IFileStorageService`, plus a document metadata model persisted in the existing EF Core database. Project- and user-level authorization will be enforced in service methods before any file is downloaded, shared, or deleted.

## Rationale

This matches the repository’s offline-first training goals and the existing architecture described in the project README. It also preserves a clean path to Azure migration by separating business logic from storage implementation. The `AppData/uploads` pattern reduces risk from directory traversal, keeps public static assets separate from user content, and lets the app produce a consistent document path structure for both local and future cloud storage.

## Alternatives Considered

- Directly storing uploaded files inside `wwwroot`: rejected because it exposes user content through static files and bypasses the authorization checks needed for secure document access.
- Saving raw file bytes directly in the database: rejected because it complicates future migration, adds large-object storage concerns, and conflicts with the project’s existing pattern of separate metadata and binary storage.
- Azure-only storage from the start: rejected because the project must remain offline and self-contained for training uses.

## Research Findings

1. Document IDs should remain integer-based to remain consistent with the current User and Project key model.
2. Category should be stored as text to keep the schema simple and match the stakeholder requirement for a predefined list of labels.
3. Generated file names should use a GUID-suffixed path and avoid using original filenames to prevent path traversal and duplicate storage collisions.
4. The secure pattern is: validate file -> generate safe storage path -> write file -> save metadata -> notify authorized users.
5. Authorization should be enforced both at page level and in service methods because the project constitution explicitly prohibits trusting the UI alone.

## Open Questions Resolved

- Storage location: local application data directory outside the web root.
- Access model: role- and project-aware permissions enforced via service logic.
- Metadata model: integer document ID, string category, and file metadata captured in the database.
- Migration path: interface abstraction and dependency injection remain the standard mechanism for future cloud swap-in.
