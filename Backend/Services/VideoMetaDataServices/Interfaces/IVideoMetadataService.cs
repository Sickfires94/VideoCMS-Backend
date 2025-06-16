using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IVideoMetadataService
    {
        public Task<VideoMetadata> addVideoMetadata(VideoMetadata videoMetadata);
        public Task<List<VideoMetadata>> getAllVideoMetadata();
        public Task<VideoMetadata> getVideoMetadataById(int id);
    }
}
