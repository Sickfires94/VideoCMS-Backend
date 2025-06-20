using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services;
using Backend.Services.RabbitMq;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Backend.Repositories.VideoMetadataRepositories
{
    public class VideoMetadataRepository : IVideoMetadataRepository
    {
        private readonly VideoManagementApplicationContext _context;

        public VideoMetadataRepository(VideoManagementApplicationContext context)
        {
            _context = context;
        }

        public async Task<VideoMetadata> addVideoMetadata(VideoMetadata videoMetadata)
        {
            // Add the new VideoMetadata object
            await _context.videoMetadatas.AddAsync(videoMetadata);
            await _context.SaveChangesAsync();

            // Now, use eager loading to return the full object, including category and videoTags
            var fullVideoMetadata = await _context.videoMetadatas
                .Include(v => v.category)       // Include category (optional)
                .Include(v => v.videoTags)      // Include videoTags (optional)
                .Include(v => v.user)           // Include user (optional)
                .FirstOrDefaultAsync(v => v.videoId == videoMetadata.videoId);

            // Optional logging for debugging
            Debug.WriteLine("***********************************");
            Debug.WriteLine("Video Metadata category: " + fullVideoMetadata?.category?.categoryName);
            Debug.WriteLine("Video Metadata categoryId: " + fullVideoMetadata?.categoryId);
            Debug.WriteLine("Video Metadata videoTags Count: " + (fullVideoMetadata?.videoTags?.Count ?? 0));

            // Return the fully loaded VideoMetadata
            return fullVideoMetadata ?? videoMetadata;
        }


        public Task<List<VideoMetadata>> getAllVideoMetadata()
        {   
            return  _context.videoMetadatas.ToListAsync();
        }

        public Task<VideoMetadata> getVideoMetadataById(int videoId)
        {
            return _context.videoMetadatas.FirstAsync(v => v.videoId == videoId);
        }

    }
}
