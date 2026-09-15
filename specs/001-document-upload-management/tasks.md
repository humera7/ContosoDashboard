# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare the local storage, configuration, and app plumbing for document uploads.

- [ ] T001 Create the document upload support structure in `ContosoDashboard/Services/`, `ContosoDashboard/Models/`, and `ContosoDashboard/Pages/`
- [ ] T002 Configure local upload directory and app settings in `ContosoDashboard/appsettings.json` and `ContosoDashboard/appsettings.Development.json`
- [ ] T003 [P] Add upload storage bootstrap and DI registration in `ContosoDashboard/Program.cs`
- [ ] T004 [P] Prepare directory creation and file validation helpers in `ContosoDashboard/Services/FileValidationService.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that must be complete before any user story work begins.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T005 Add the document storage abstraction contract in `ContosoDashboard/Services/IFileStorageService.cs`
- [ ] T006 [P] Implement the local filesystem storage backend in `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T007 [P] Extend the EF Core context with document DbSets and relationships in `ContosoDashboard/Data/ApplicationDbContext.cs`
- [ ] T008 Create the main document model in `ContosoDashboard/Models/Document.cs`
- [ ] T009 [P] Create the sharing and audit models in `ContosoDashboard/Models/DocumentShare.cs` and `ContosoDashboard/Models/AuditEvent.cs`
- [ ] T010 Implement the document service contract and core logic in `ContosoDashboard/Services/IDocumentService.cs` and `ContosoDashboard/Services/DocumentService.cs`
- [ ] T011 Add authorization checks and ownership validation in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T012 Configure a queue-based scan job contract for future Azure Function processing in `ContosoDashboard/Services/DocumentScanQueueMessage.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - Upload and organize work documents (Priority: P1) 🎯 MVP

**Goal**: Let employees upload valid files, assign metadata, and see them in personal or project document lists.

**Independent Test Criteria**: A logged-in user can upload a valid file, enter the required metadata, and confirm the record appears in the My Documents or project document list without extra setup.

### Implementation for User Story 1

- [ ] T013 [P] [US1] Add upload form fields and category selection in `ContosoDashboard/Pages/Documents.razor`
- [ ] T014 [P] [US1] Add a file upload modal or dedicated upload page in `ContosoDashboard/Pages/Documents.razor` and `ContosoDashboard/Pages/ProjectDetails.razor`
- [ ] T015 [US1] Implement upload validation, file size checks, and extension allow-list logic in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T016 [US1] Implement safe file persistence with GUID-based names and metadata save flow in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T017 [US1] Add document list sorting and filtering in `ContosoDashboard/Pages/Documents.razor`
- [ ] T018 [US1] Add document search by title, description, tags, and uploader in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Pages/Documents.razor`
- [ ] T019 [US1] Add upload confirmation and error messages in `ContosoDashboard/Pages/Documents.razor`
- [ ] T020 [US1] Create a document status model and initial scan-pending state in `ContosoDashboard/Models/Document.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - Access project documents with role-aware permissions (Priority: P1)

**Goal**: Ensure project members can access authorized documents and non-members cannot view restricted files.

**Independent Test Criteria**: A project member can view and download project documents while a non-member receives a permission error when trying to access the same document or project list.

### Implementation for User Story 2

- [ ] T021 [P] [US2] Add project document listing to `ContosoDashboard/Pages/ProjectDetails.razor`
- [ ] T022 [P] [US2] Add user and project authorization checks in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T023 [US2] Implement download and preview authorization flow in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T024 [US2] Add document access enforcement for project members, managers, and administrators in `ContosoDashboard/Pages/ProjectDetails.razor` and `ContosoDashboard/Services/DocumentService.cs`
- [ ] T025 [US2] Add secure document download handling to `ContosoDashboard/Pages/Documents.razor` or an associated document controller page
- [ ] T026 [US2] Add project-scoped document queries and filtering to `ContosoDashboard/Services/DocumentService.cs`

**Checkpoint**: At this point, User Stories 1 and 2 should both work independently.

---

## Phase 5: User Story 3 - Share and notify colleagues about relevant documents (Priority: P2)

**Goal**: Allow document owners to share files and deliver in-app notifications to recipients.

**Independent Test Criteria**: A document owner shares a file with a colleague and the recipient sees the shared document and notification without needing other feature setup.

### Implementation for User Story 3

- [ ] T027 [P] [US3] Create the sharing UI in `ContosoDashboard/Pages/Documents.razor`
- [ ] T028 [US3] Implement share record creation and validation in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T029 [US3] Wire notification creation for shared documents in `ContosoDashboard/Services/NotificationService.cs`
- [ ] T030 [US3] Add recipient "Shared with Me" listing in `ContosoDashboard/Pages/Documents.razor`
- [ ] T031 [US3] Add share and revoke flows with authorization checks in `ContosoDashboard/Services/DocumentService.cs`

**Checkpoint**: User Story 3 should be independently functional and not depend on the rest of the feature being complete.

---

## Phase 6: User Story 4 - Review activity and maintain auditability (Priority: P3)

**Goal**: Provide administrators with activity logs and reportable document metrics.

**Independent Test Criteria**: An administrator can open the reporting area and review uploads, downloads, share actions, and deletions across the document set.

### Implementation for User Story 4

- [ ] T032 [P] [US4] Add an admin report view in `ContosoDashboard/Pages/Index.razor` or a dedicated admin document reporting page
- [ ] T033 [US4] Log all document actions in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Models/AuditEvent.cs`
- [ ] T034 [US4] Generate summary report data for uploaders, file types, and access patterns in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T035 [US4] Expose admin reporting UI and filters in `ContosoDashboard/Pages/Index.razor` or a new reporting page

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Review, hardening, and final validation across the full feature.

- [ ] T036 [P] Add dashboard summary card and recent documents widget in `ContosoDashboard/Pages/Index.razor`
- [ ] T037 [P] Add task/project document attachments and metadata linkage in `ContosoDashboard/Pages/Tasks.razor` and `ContosoDashboard/Pages/ProjectDetails.razor`
- [ ] T038 [P] Add file scan queue worker stub or Azure Function contract in a future `ContosoDashboard.Functions/` or equivalent worker project
- [ ] T039 [P] Add document preview support for PDF and image files in `ContosoDashboard/Pages/Documents.razor`
- [ ] T040 Validate the local upload flow against the quickstart scenarios in `specs/001-document-upload-management/quickstart.md`
- [ ] T041 Run a security review for access-control gaps, path sanitization, and storage isolation in `ContosoDashboard/Services/DocumentService.cs` and `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T042 Final cleanup and documentation updates in `README.md` and `specs/001-document-upload-management/`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational completion
  - User stories can proceed in parallel if the team has capacity
  - Default execution order is P1 → P2 → P3, with P1 stories strongly prioritized
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - no dependencies on other stories
- **User Story 2 (P1)**: Depends on the upload and access model from US1 but remains independently testable
- **User Story 3 (P2)**: Depends on the document model and basic authorization from US1/US2
- **User Story 4 (P3)**: Depends on document activity logging and report-ready data from the prior stories

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- User Story 1 implementation tasks can be split across upload UI, validation, and storage work
- User Story 2 access-control checks can run in parallel with the US1 document list work after the shared service contract is in place
- The admin reporting work can proceed independently once audit logging is available

---

## Parallel Example: User Story 1

```bash
# Parallel work for upload and storage setup
Task: "Add upload form fields and category selection in ContosoDashboard/Pages/Documents.razor"
Task: "Implement safe file persistence with GUID-based names in ContosoDashboard/Services/LocalFileStorageService.cs"
Task: "Add upload validation and extension filtering in ContosoDashboard/Services/DocumentService.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate the upload and search workflow independently
5. Deploy or demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → foundation ready
2. Add User Story 1 → upload and organization flow
3. Add User Story 2 → project-scoped authorization and downloads
4. Add User Story 3 → sharing and notifications
5. Add User Story 4 → reporting and audit review
6. Finish with polish and cross-cutting validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3 / User Story 4
3. Complete integration and polish once each story passes its validation criteria

---

## Notes

- [P] tasks = different files and no dependencies
- [Story] labels map tasks to specific user stories for traceability
- Each user story should be independently completable and testable
- Keep validation focused on the acceptance criteria in `spec.md`
- Ensure the upload pipeline keeps the app secure and offline-capable while preserving a migration-ready storage abstraction
