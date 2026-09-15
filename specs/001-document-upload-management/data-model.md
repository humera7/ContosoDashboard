# Data Model: Document Upload and Management

## Overview

The feature adds document metadata and file-sharing tracking to the existing dashboard schema. The model continues the project’s current pattern of integer keys and EF Core navigation properties while isolating binary content from the database.

## Entities

### Document

Represents a single uploaded file and its metadata.

| Field | Type | Constraints | Notes |
|---|---|---|---|
| DocumentId | int | PK, required | Consistent with existing integer IDs |
| Title | string | Required, max 255 | User-facing title |
| Description | string | Optional, max 2000 | Rich text not required for MVP |
| Category | string | Required, max 100 | One of the predefined category values |
| FileNameOriginal | string | Required, max 255 | Original file name for display |
| StoredFileName | string | Required, max 255 | GUID-based safe file name |
| StoredRelativePath | string | Required, max 500 | Relative path used by the storage service |
| ContentType | string | Required, max 255 | MIME type |
| FileSizeBytes | long | Required | Up to 25 MB |
| UploadedByUserId | int | FK to User, required | Uploader identity |
| ProjectId | int? | FK to Project, optional | Null for personal documents |
| CreatedDate | DateTime | Required | Upload timestamp |
| UpdatedDate | DateTime | Required | Last metadata update |
| Tags | string | Optional, max 500 | Comma-delimited or simple search-friendly text |
| IsDeleted | bool | Required, default false | Supports soft delete if needed |

Relationships:
- Many documents belong to one uploader (`User`)
- Many documents may belong to one project (`Project`), or be personal with no project
- One document may have many share records and many audit events

### DocumentShare

Represents a user-to-user document sharing record.

| Field | Type | Constraints | Notes |
|---|---|---|---|
| DocumentShareId | int | PK | Unique share record |
| DocumentId | int | FK to Document | Shared document |
| SharedWithUserId | int | FK to User | Recipient |
| SharedByUserId | int | FK to User | Owner or project manager |
| SharedDate | DateTime | Required | Share timestamp |
| Message | string | Optional | Optional notes for the recipient |

Relationships:
- Many share records point to one document
- One recipient user may receive many shared documents
- One owner may share many documents

### AuditEvent

Represents a document activity record used for admin reporting.

| Field | Type | Constraints | Notes |
|---|---|---|---|
| AuditEventId | int | PK | Unique audit row |
| DocumentId | int? | FK to Document, optional | Null for non-document events if needed |
| UserId | int | FK to User | Acting user |
| ActionType | string | Required, max 100 | Upload, download, share, delete, replace |
| ActionDate | DateTime | Required | Audit timestamp |
| Details | string | Optional, max 1000 | Free-form description |

Relationships:
- One user can trigger many audit events
- One document may have many audit events

## Validation Rules

- Title is required and cannot be empty after trimming.
- Category must match one of the approved values: Project Documents, Team Resources, Personal Files, Reports, Presentations, Other.
- File size must be <= 25 MB.
- File type must be in an allowed set: PDF, Office documents, text files, JPEG, PNG.
- Document storage path must be generated before metadata is persisted.
- Access checks must happen before download, delete, or share actions proceed.

## State Transitions

Document states are conceptual rather than a separate enum for the first implementation, but the lifecycle is:

1. Draft metadata / upload started
2. File validated and stored
3. Metadata persisted
4. Visible in user/project lists
5. Shared or updated
6. Deleted after confirmation

## Relationship Notes

This feature follows the project’s current model conventions:
- integer primary keys for business entities
- navigation properties for EF Core relationships
- explicit authorization checks in the service layer, not only in the page layer
- local file storage separated from database metadata
