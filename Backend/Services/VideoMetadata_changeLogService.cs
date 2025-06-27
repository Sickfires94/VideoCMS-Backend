using Backend.DTOs;
using Backend.Repositories.Interface;
using Backend.Services.Interfaces;

namespace Backend.Services
{
    public class VideoMetadata_changeLogService : IVideoMetadata_changeLogService
    {
        private readonly IVideoMetadata_changelogRepository _repository;

        public VideoMetadata_changeLogService(IVideoMetadata_changelogRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<VideoMetadataChangeLog>> getLogsByVideoId(int videoId)
        {
            return _repository.getLogsByVideoId(videoId);
        }

        public Task LogChange(VideoMetadata video)
        {
            return _repository.LogChange(video);
        }
    }
}
