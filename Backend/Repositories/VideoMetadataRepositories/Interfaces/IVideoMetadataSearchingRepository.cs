using Backend.DTOs;

namespace Backend.Repositories.VideoMetadataRepositories.Interfaces
{
    public interface IVideoMetadataSearchingRepository
    {
        // Searching Functionality
        public Task<List<VideoMetadata>> searchVideoMetadataByName(string videoName);
        public Task<List<VideoMetadata>> searchVideoMetadataByDescription(string videoDescription);
        public Task<List<VideoMetadata>> searchVideoMetadataByTag(string tagName);
        public Task<List<VideoMetadata>> searchVideoMetadataByCategory(string categoryName);

    }
}
