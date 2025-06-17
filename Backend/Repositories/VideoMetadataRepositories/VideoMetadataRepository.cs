using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services;
using Backend.Services.RabbitMq;
using Microsoft.EntityFrameworkCore;

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
