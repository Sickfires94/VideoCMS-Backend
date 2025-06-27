using Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Diagnostics;
using System.Security.Claims;

namespace Backend.Interceptors
{
    public class VideoMetadataAuditInterceptor : SaveChangesInterceptor
    {
        // If you need to access HttpContext (for current user), inject IHttpContextAccessor
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<VideoMetadataAuditInterceptor> _logger;

        public VideoMetadataAuditInterceptor(IHttpContextAccessor httpContextAccessor = null, ILogger<VideoMetadataAuditInterceptor> logger = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            LogChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            LogChanges(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void LogChanges(DbContext? context)
        {

            Debug.WriteLine("Logging Changes");
            if (context == null) return;

            var auditEntries = new List<VideoMetadataChangeLog>();
            var currentUserName = GetCurrentUserName();



            foreach (var entry in context.ChangeTracker.Entries<VideoMetadata>())
            {
               

                _logger.LogInformation("Entry: {entry}", entry);    

                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || !(entry is VideoMetadata))
                {
                    continue;
                }

                
                var auditEntry = new VideoMetadataChangeLog
                {
                    VideoId = (int)entry.Property("videoId").CurrentValue, // Assuming videoId is set for all states
                    ChangeTime = DateTime.UtcNow,
                    // UpdatedBy = currentUserId,
                    UpdatedByUserName = currentUserName
                };

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.ChangeType = "Insert";
                        auditEntry.UpdatedVideoName = (string?)entry.Property("videoName").CurrentValue;
                        auditEntry.UpdatedVideoDescription = (string?)entry.Property("videoDescription").CurrentValue;
                        auditEntry.UpdatedVideoUrl = (string?)entry.Property("videoUrl").CurrentValue;
                        auditEntry.UpdatedCategoryId = (int?)entry.Property("categoryId").CurrentValue;
                        break;

                    case EntityState.Modified:
                        auditEntry.ChangeType = "Update";

                        // Capture changed properties
                        foreach (var property in entry.Properties)
                        {
                            if (property.IsModified)
                            {
                                switch (property.Metadata.Name)
                                {
                                    case "videoName":
                                        auditEntry.PreviousVideoName = (string?)property.OriginalValue;
                                        auditEntry.UpdatedVideoName = (string?)property.CurrentValue;
                                        break;
                                    case "videoDescription":
                                        auditEntry.PreviousVideoDescription = (string?)property.OriginalValue;
                                        auditEntry.UpdatedVideoDescription = (string?)property.CurrentValue;
                                        break;
                                    case "videoUrl": // Assuming VideoUrl exists
                                        auditEntry.PreviousVideoUrl = (string?)property.OriginalValue;
                                        auditEntry.UpdatedVideoUrl = (string?)property.CurrentValue;
                                        break;
                                    case "categoryId":
                                        auditEntry.PreviousCategoryId = (int?)property.OriginalValue;
                                        auditEntry.UpdatedCategoryId = (int?)property.CurrentValue;
                                        break;
                                        // Add cases for other properties you want to log
                                }
                            }
                        }
                        break;

                    case EntityState.Deleted:
                        auditEntry.ChangeType = "Delete";
                        auditEntry.VideoId = (int)entry.Property("videoId").OriginalValue; // Use original ID for deleted
                        auditEntry.PreviousVideoName = (string?)entry.Property("videoName").OriginalValue;
                        auditEntry.PreviousVideoDescription = (string?)entry.Property("videoDescription").OriginalValue;
                        auditEntry.PreviousVideoUrl = (string?)entry.Property("videoUrl").OriginalValue; // Assuming VideoUrl exists
                        auditEntry.PreviousCategoryId = (int?)entry.Property("categoryId").OriginalValue;
                        break;
                }
                auditEntries.Add(auditEntry);
            }

            // Add the audit entries to the context to be saved
            if (auditEntries.Any())
            {
                // IMPORTANT: Add these to the same context. When SaveChanges() is called
                // again, the interceptor will be re-triggered.
                // To avoid issues, ensure your audit log table insertion doesn't
                // itself cause more audit logs (e.g., if you were to audit AuditLogs).
                context.Set<VideoMetadataChangeLog>().AddRange(auditEntries);
            }
        }

        private int? GetCurrentUserId()
        {
            // Implement logic to get the current user ID.
            // This depends on your authentication and authorization setup.
            // Example using HttpContextAccessor (for web applications):
            if (_httpContextAccessor?.HttpContext?.User?.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
            {
                // Assuming your User ID is stored in a ClaimTypes.NameIdentifier
                var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return null; // Or a default 'system' user ID
        }

        private string? GetCurrentUserName()
        {
            if (_httpContextAccessor?.HttpContext?.User?.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
            {
                // Assuming the user's name is stored in ClaimTypes.Name
                var userNameClaim = identity.FindFirst(ClaimTypes.Name);
                if (userNameClaim != null)
                {
                    return userNameClaim.Value;
                }
            }
            return null; // Return null if user name is not found or not authenticated
        }
    }
}
