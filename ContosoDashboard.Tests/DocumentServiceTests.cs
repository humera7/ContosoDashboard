using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContosoDashboard.Tests;

public class DocumentServiceTests
{
    [Fact]
    public async Task UploadDocument_ShouldPersistMetadata_AndCreateOwnedRecord()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var user = new User
        {
            UserId = 1,
            Email = "u1@example.com",
            DisplayName = "User One",
            Role = UserRole.Employee,
            Department = "Engineering"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var storage = new LocalFileStorageService();
        var service = new DocumentService(context, storage);
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });

        var result = await service.UploadAsync(
            user.UserId,
            "Quarterly Report",
            "Summary report",
            "Reports",
            "report.pdf",
            stream,
            "application/pdf",
            null,
            "2025,finance"
        );

        Assert.NotNull(result);
        Assert.Equal("Quarterly Report", result.Title);
        Assert.Equal("Reports", result.Category);
        Assert.Equal(user.UserId, result.UploadedByUserId);
        Assert.False(string.IsNullOrWhiteSpace(result.StoredRelativePath));
        Assert.Equal(1, await context.Documents.CountAsync());
    }

    [Fact]
    public async Task GetProjectDocumentsAsync_ShouldRejectUnauthorizedUser()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var manager = new User { UserId = 1, Email = "mgr@test.com", DisplayName = "Manager", Role = UserRole.ProjectManager };
        var member = new User { UserId = 2, Email = "member@test.com", DisplayName = "Member", Role = UserRole.Employee };
        var outsider = new User { UserId = 3, Email = "outsider@test.com", DisplayName = "Outsider", Role = UserRole.Employee };
        context.Users.AddRange(manager, member, outsider);

        var project = new Project
        {
            ProjectId = 1,
            Name = "Alpha",
            ProjectManagerId = manager.UserId,
            Description = "Test project",
            Status = ProjectStatus.Active,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
        context.Projects.Add(project);
        context.ProjectMembers.Add(new ProjectMember { ProjectId = project.ProjectId, UserId = member.UserId, Role = "Developer" });
        await context.SaveChangesAsync();

        var storage = new LocalFileStorageService();
        var service = new DocumentService(context, storage);

        var docs = await service.GetProjectDocumentsAsync(project.ProjectId, outsider.UserId);

        Assert.Empty(docs);
    }
}
