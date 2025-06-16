using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IVideoMetadataSearchService
    {
        public Task<List<VideoMetadata>> searchVideoMetadataByName(string videoName);
        public Task<List<VideoMetadata>> searchVideoMetadataByDescription(string videoDescription);
        public Task<List<VideoMetadata>> searchVideoMetadataByTag(string tagName);
        public Task<List<VideoMetadata>> searchVideoMetadataByCategory(string categoryName);


       
    }
}
