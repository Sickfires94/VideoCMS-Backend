using Backend.DTOs;

namespace Backend.Repositories.VideoMetadataRepositories.Interfaces
{
    public interface IVideoMetadataRepository
    {
        public Task<VideoMetadata> addVideoMetadata(VideoMetadata videoMetadata);
        public Task<List<VideoMetadata>> getAllVideoMetadata();
        public Task<VideoMetadata> getVideoMetadataById(int id);
        public Task<VideoMetadata> updateVideoMetadata(int id, VideoMetadata videoMetadata);
        public Task deleteVideoMetadata(int id);
    }
}
