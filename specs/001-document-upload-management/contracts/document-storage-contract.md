# Contract: File Storage and Document Access

## Purpose

This contract defines how uploaded document content is stored and retrieved while preserving a clear boundary between metadata operations and binary file operations.

## Storage Interface

```csharp
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string relativePath);
    Task DeleteAsync(string relativePath);
    Task<Stream> DownloadAsync(string relativePath);
    Task<string> GetUrlAsync(string relativePath, TimeSpan expiration);
}
```

## Contract Rules

### UploadAsync
- Input: validated stream, sanitized file name, MIME type, and safe relative storage path.
- Precondition: the file has passed extension and size validation.
- Postcondition: the file is written to storage and the returned path is stable for later retrieval and deletion.

### DeleteAsync
- Input: the stored relative path for an existing document.
- Precondition: the caller is authorized to remove the document.
- Postcondition: the physical file is removed and no orphaned storage object remains.

### DownloadAsync
- Input: relative path for a stored document.
- Precondition: the caller has authorization to access the document.
- Postcondition: a stream is returned for the file content. The call must not expose files outside the approved storage directory.

### GetUrlAsync
- Input: relative path and expiration window.
- Precondition: the caller is permitted to access the file.
- Postcondition: a time-limited access URL or tokenized path is generated for future integration with blob or object storage.

## Security Constraints

- Never trust a user-supplied file name to become part of the stored path.
- Always generate a GUID-based safe file name before save.
- Keep file content outside the public web root.
- Require service-layer authorization before download or deletion operations proceed.

## Compatibility Notes

The local implementation uses a filesystem-backed store; the Azure implementation can swap in later without changing the document business logic, database schema, or page interactions.
