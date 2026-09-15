# Implementation Plan: Document Upload and Management

**Branch**: `001-document-upload-management` | **Date**: 2026-09-15 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from [spec.md](spec.md)

## Summary

This feature adds a secure document management workflow to the existing Blazor Server dashboard. The design centers on a file storage abstraction, a document metadata model, role-aware access rules, and an asynchronous security pipeline for malware scanning. The implementation will keep the local training deployment offline-first while preserving a clear migration path to Azure-backed storage through an `IFileStorageService` contract and a queue-driven Azure Functions processing layer for antivirus checks.

## Technical Context

**Language/Version**: C# .NET 8.0 / ASP.NET Core 8.0, Blazor Server, Azure Functions for background scan jobs  
**Primary Dependencies**: Entity Framework Core, Bootstrap 5, local filesystem access, existing custom auth provider, Azure Storage Queue SDK (future Azure deployment), Azure Functions runtime  
**Storage**: SQL Server LocalDB or local SQLite-compatible EF context for metadata; filesystem storage under a dedicated application data folder for uploaded files; Azure Queue Storage for async scan messages in future cloud deployment  
**Testing**: .NET test projects with xUnit, plus focused authorization and service-layer validation, queue message contract verification for scan jobs  
**Target Platform**: Windows desktop dev environment with browser access to the Blazor app; future Azure-hosted background scan worker  
**Project Type**: Web application with async background processing  
**Performance Goals**: Upload under 30 seconds for files up to 25 MB, search and list responses under 2 seconds for up to 500 documents, queued scan jobs processed asynchronously without blocking user upload completion  
**Constraints**: offline-only training build, secure file storage outside `wwwroot`, no direct user-supplied file names in path generation, int-based document IDs, string-based category values, malware scanning must not hold the end-user request open  
**Scale/Scope**: Small team dashboard with role-aware project and personal document access; supports a few hundred documents and a small number of shared recipients, with a background queue for scan tasks

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

The feature aligns with the repository constitution and does not require a variance exception.

- **Security by Default**: passes. This feature introduces explicit authorization checks for uploads, downloads, sharing, and document visibility, and it keeps files outside the public web root.
- **Training-Grade Simplicity**: passes. The design uses a straightforward document model, storage abstraction, and minimal added pages without introducing unnecessary complexity.
- **Test-First and Behavior-Guided Change**: passes. The feature specification defines explicit acceptance criteria and security behaviors for upload, access control, and sharing scenarios.
- **Data Integrity and Access Boundaries**: passes. The design requires project membership, user ownership, and role checks before access or mutation.
- **Offline-First Learning with Explicit Migration Paths**: passes. The storage abstraction and local filesystem pattern satisfy the offline requirement while allowing future Azure adaptation.

## Project Structure

### Documentation (this feature)

```text
specs/001-document-upload-management/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── document-storage-contract.md
└── checklists/
    └── requirements.md
```

### Source Code (repository root)

```text
ContosoDashboard/
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── User.cs
│   ├── Project.cs
│   ├── TaskItem.cs
│   ├── Notification.cs
│   ├── ProjectMember.cs
│   └── ...
├── Services/
│   ├── DashboardService.cs
│   ├── NotificationService.cs
│   ├── ProjectService.cs
│   ├── TaskService.cs
│   ├── UserService.cs
│   └── ...
├── Pages/
│   ├── Index.razor
│   ├── Projects.razor
│   ├── ProjectDetails.razor
│   ├── Tasks.razor
│   ├── Login.cshtml
│   └── ...
├── Shared/
├── wwwroot/
└── Program.cs
```

**Structure Decision**: The implementation will extend the existing layered architecture by adding document metadata entities in the `Models` and `Data` layers, a storage abstraction and document service in `Services`, a queue-driven background scan worker for malware checks, and the user-facing document pages and dashboard widgets under `Pages` and `Shared`.

### Background Scan Job Design

The upload workflow will treat malware scanning as an asynchronous follow-up process after a file is safely stored. The upload service will validate the file, generate a secure storage path, save the file to the local file system, persist the document metadata, and then enqueue a document scan message. This keeps the user experience responsive while ensuring the file is not considered approved until the background scan completes.

For future Azure deployment, the design will use an Azure Function with a Queue Storage trigger. The trigger will receive a message containing the document identifier, storage path, and scan request metadata, then fetch the file from the storage abstraction, run the antivirus or scanning workflow, and update the document status as `clean`, `quarantined`, or `failed`. The same pattern can be emulated locally with a lightweight queue implementation or a local Azure Storage emulator during training.

The queue message contract should include:
- `DocumentId`
- `OwnerUserId`
- `StoragePath`
- `ContentType`
- `FileName`
- `UploadedAt`
- `CorrelationId`

This allows the scan worker to be independent from the Blazor UI flow, while preserving clear auditability and a staged approval model for uploaded content.

## Complexity Tracking

No constitution violations require justification.
