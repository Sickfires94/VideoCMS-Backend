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
            await _context.videoMetadatas.AddAsync(videoMetadata);
            await _context.SaveChangesAsync();

            return videoMetadata;
        }

        public async Task deleteVideoMetadata(int id)
        {
            await _context.videoMetadatas.Where(v => v.videoId == id).ExecuteDeleteAsync();
        }

        public async Task<List<VideoMetadata>> getAllVideoMetadata()
        {   
            return await _context.videoMetadatas
                .Include(v => v.user)
                .Include(v => v.videoTags)
                .Include(v => v.category)
                .ToListAsync();
        }

        public async Task<VideoMetadata?> getVideoMetadataById(int videoId)
        {
            return await _context.videoMetadatas
                .Include(v => v.category)       // Include category (optional)
                .Include(v => v.videoTags)      // Include videoTags (optional)
                .Include(v => v.user)           // Include user (optional)
                .FirstOrDefaultAsync<VideoMetadata>(v => v.videoId == videoId) ?? null;
        }

        public async Task<VideoMetadata> updateVideoMetadata(int id, VideoMetadata newVideo)
        {
            VideoMetadata video = await _context.videoMetadatas
                .Include(v => v.category)  
                .Include(v => v.videoTags)  
                .Include(v => v.user)
                .SingleAsync(v => v.videoId == id);

            video.videoName = newVideo.videoName;
            video.videoDescription = newVideo.videoDescription;
            video.category = newVideo.category;
            video.videoName = newVideo.videoName;
            video.videoTags = newVideo.videoTags;
            video.videoUpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            Debug.WriteLine("User: " + video.user.userName);

            return video;
        }
    }
}
