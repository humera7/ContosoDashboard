using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storageService;
    private const long MaxFileSizeBytes = 25 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".ppt",
        ".pptx",
        ".txt",
        ".jpg",
        ".jpeg",
        ".png"
    };

    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Project Documents",
        "Team Resources",
        "Personal Files",
        "Reports",
        "Presentations",
        "Other"
    };

    public DocumentService(ApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<Document> UploadAsync(int uploadedByUserId, string title, string? description, string category, string originalFileName, Stream fileStream, string contentType, int? projectId, string? tags)
    {
        var user = await _context.Users.FindAsync(uploadedByUserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        title = title?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException("Document title is required.");
        }

        if (!AllowedCategories.Contains(category))
        {
            throw new InvalidOperationException("Invalid document category.");
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new InvalidOperationException("File name is required.");
        }

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        if (fileStream.Length == 0 || fileStream.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException("File size must be between 1 byte and 25 MB.");
        }

        if (projectId.HasValue)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId.Value);

            if (project == null)
            {
                throw new InvalidOperationException("Project not found.");
            }

            var isManager = project.ProjectManagerId == uploadedByUserId;
            var isMember = project.ProjectMembers.Any(pm => pm.UserId == uploadedByUserId);
            if (!isManager && !isMember)
            {
                throw new UnauthorizedAccessException("User is not authorized to upload to this project.");
            }
        }

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine(uploadedByUserId.ToString(), projectId?.ToString() ?? "personal", safeFileName).Replace('\\', '/');

        await _storageService.UploadAsync(fileStream, safeFileName, contentType, relativePath);

        var document = new Document
        {
            Title = title,
            Description = description,
            Category = category,
            FileNameOriginal = originalFileName,
            StoredFileName = safeFileName,
            StoredRelativePath = relativePath,
            ContentType = contentType,
            FileSizeBytes = fileStream.Length,
            UploadedByUserId = uploadedByUserId,
            ProjectId = projectId,
            Tags = tags,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            Status = DocumentStatus.PendingScan
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        _context.AuditEvents.Add(new AuditEvent
        {
            DocumentId = document.DocumentId,
            UserId = uploadedByUserId,
            ActionType = "Upload",
            ActionDate = DateTime.UtcNow,
            Details = $"Uploaded {originalFileName}"
        });

        await _context.SaveChangesAsync();

        return document;
    }

    public async Task<List<Document>> GetUserDocumentsAsync(int userId)
    {
        return await _context.Documents
            .Where(d => d.UploadedByUserId == userId && !d.IsDeleted)
            .Include(d => d.Project)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Document>> GetProjectDocumentsAsync(int projectId, int requestingUserId)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null)
        {
            return new List<Document>();
        }

        var isManager = project.ProjectManagerId == requestingUserId;
        var isMember = project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);
        if (!isManager && !isMember)
        {
            return new List<Document>();
        }

        return await _context.Documents
            .Where(d => d.ProjectId == projectId && !d.IsDeleted)
            .Include(d => d.Project)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Document>> SearchAsync(int userId, string query)
    {
        var normalized = query.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return await GetUserDocumentsAsync(userId);
        }

        var allowedIds = await _context.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        var projectIds = allowedIds.Concat(new[] { -1 }).Distinct().ToList();

        return await _context.Documents
            .Where(d => !d.IsDeleted
                && ((d.UploadedByUserId == userId)
                    || (d.ProjectId.HasValue && projectIds.Contains(d.ProjectId.Value))
                    || (d.Shares.Any(s => s.SharedWithUserId == userId))))
            .Where(d =>
                d.Title.Contains(normalized)
                || (d.Description != null && d.Description.Contains(normalized))
                || (d.Tags != null && d.Tags.Contains(normalized))
                || d.FileNameOriginal.Contains(normalized))
            .Include(d => d.Project)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<Document?> GetByIdAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .ThenInclude(p => p!.ProjectMembers)
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null)
        {
            return null;
        }

        var isOwner = document.UploadedByUserId == requestingUserId;
        var isProjectManager = document.Project != null && document.Project.ProjectManagerId == requestingUserId;
        var isProjectMember = document.Project != null && document.Project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);
        var isSharedToUser = await _context.DocumentShares.AnyAsync(s => s.DocumentId == documentId && s.SharedWithUserId == requestingUserId);

        if (!isOwner && !isProjectManager && !isProjectMember && !isSharedToUser)
        {
            return null;
        }

        return document;
    }

    public async Task<bool> DeleteAsync(int documentId, int requestingUserId)
    {
        var document = await _context.Documents
            .Include(d => d.Project)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null)
        {
            return false;
        }

        var isOwner = document.UploadedByUserId == requestingUserId;
        var isManager = document.Project != null && document.Project.ProjectManagerId == requestingUserId;
        if (!isOwner && !isManager)
        {
            return false;
        }

        document.IsDeleted = true;
        document.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _storageService.DeleteAsync(document.StoredRelativePath);

        _context.AuditEvents.Add(new AuditEvent
        {
            DocumentId = document.DocumentId,
            UserId = requestingUserId,
            ActionType = "Delete",
            ActionDate = DateTime.UtcNow,
            Details = "Document deleted"
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Document?> UpdateMetadataAsync(int documentId, int requestingUserId, string? title, string? description, string? category, string? tags)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.IsDeleted);

        if (document == null || document.UploadedByUserId != requestingUserId)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            document.Title = title.Trim();
        }

        document.Description = description;
        if (!string.IsNullOrWhiteSpace(category))
        {
            if (!AllowedCategories.Contains(category))
            {
                throw new InvalidOperationException("Invalid document category.");
            }
            document.Category = category;
        }

        document.Tags = tags;
        document.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<List<Document>> GetSharedWithUserAsync(int userId)
    {
        return await _context.DocumentShares
            .Where(s => s.SharedWithUserId == userId)
            .Select(s => s.Document)
            .Where(d => !d.IsDeleted)
            .Include(d => d.Project)
            .ToListAsync();
    }
}
