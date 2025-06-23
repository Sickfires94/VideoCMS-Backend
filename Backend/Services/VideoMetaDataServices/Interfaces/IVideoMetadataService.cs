using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IVideoMetadataService
    {
        public Task<VideoMetadata> addVideoMetadata(VideoMetadata videoMetadata);
        public Task<List<VideoMetadata>> getAllVideoMetadata();
        public Task<VideoMetadata> getVideoMetadataById(int id);
        public Task<VideoMetadata> updateVideoMetadata(int id, VideoMetadata video);
        public Task deleteVideoMetadata(int id);
    }
}
