using Backend.DTOs;
using Backend.Repositories.Interface;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class VideoMetadata_changelogRepository : IVideoMetadata_changelogRepository
    {

        private readonly VideoManagementApplicationContext _context;

        public VideoMetadata_changelogRepository(VideoManagementApplicationContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VideoMetadataChangeLog>> getLogsByVideoId(int videoId)
        {
            return await _context.VideoMetadataChangeLogs
                .Where(v => v.VideoId == videoId)
                .OrderBy(v => v.ChangeTime)
                .ToListAsync();
        }
    }
}
