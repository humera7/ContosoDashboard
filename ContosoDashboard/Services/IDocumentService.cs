using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public interface IDocumentService
{
    Task<Document> UploadAsync(int uploadedByUserId, string title, string? description, string category, string originalFileName, Stream fileStream, string contentType, int? projectId, string? tags);
    Task<List<Document>> GetUserDocumentsAsync(int userId);
    Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId);
    Task<List<Document>> SearchAsync(int userId, string query);
    Task<Document?> GetByIdAsync(int documentId, int requestingUserId);
    Task<bool> DeleteAsync(int documentId, int requestingUserId);
    Task<Document?> UpdateMetadataAsync(int documentId, int requestingUserId, string? title, string? description, string? category, string? tags);
    Task<List<Document>> GetSharedWithUserAsync(int userId);
}
