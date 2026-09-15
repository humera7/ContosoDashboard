# Quickstart Validation Guide

## Prerequisites

- .NET 8 SDK installed
- Local SQL Server LocalDB or the repo’s configured local database environment
- Application running in the developer environment
- At least one authorized user account available through the mock login flow

## Validation Scenarios

### 1. Upload a valid document

1. Start the application with `dotnet run` from the project root.
2. Sign in as a project member or manager.
3. Open the document upload page or the task/project document area.
4. Select a valid PDF or Office document under 25 MB.
5. Enter a title, choose a category, and optionally assign a project.
6. Submit the upload.

Expected outcome:
- The server accepts the file.
- A confirmation message appears.
- The document appears in the user’s documents list and, if project-linked, in the project document view.

### 2. Validate rejection for invalid input

1. Attempt to upload a file larger than 25 MB.
2. Attempt to upload an unsupported file type such as an executable.
3. Try uploading without a required title or category.

Expected outcome:
- The system blocks the upload.
- Error messages clearly identify the reason.
- No incomplete document record is created.

### 3. Verify project authorization

1. Sign in as a project member and open the project documents view.
2. Confirm that project documents are visible.
3. Sign out and log in as a user not assigned to the project.
4. Attempt to open the document URL directly or browse the project list.

Expected outcome:
- The authorized user sees the documents.
- The unauthorized user is denied access and cannot view the document metadata or file contents.

### 4. Test sharing and notification flow

1. Upload a document as a document owner.
2. Share it with a specific user.
3. Sign in as the share recipient.

Expected outcome:
- The recipient sees the document in a shared-with-me area.
- An in-app notification is displayed or available in the notification center.

### 5. Verify updates and deletion

1. Edit the document metadata after upload.
2. Replace the file with a new version.
3. Delete the document after confirmation.

Expected outcome:
- Metadata changes are saved.
- The new file version replaces the original record consistently.
- Deleted documents are removed only after confirmation and are no longer accessible to authorized users.

### 6. Check dashboard integration

1. Open the home dashboard.
2. Look for the recent documents widget and summary count.

Expected outcome:
- The widget lists the user’s latest documents.
- The summary card includes the document count without breaking the dashboard layout.

## Expected Observations

- Upload and list operations remain within the defined performance targets.
- Security boundaries work even if a user manually changes the route or query parameters.
- The file storage path remains outside `wwwroot` and uses safe, generated names.
