using Backend.DTOs;
using Backend.Repositories;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services.VideoMetaDataServices.Interfaces;

namespace Backend.Services.VideoMetaDataServices
{
    public class VideoMetadataSearchService : IVideoMetadataSearchService
    {

        private readonly IVideoMetadataSearchingRepository _videoMetadataSearchingRepository;

        public VideoMetadataSearchService(IVideoMetadataSearchingRepository repository)
        {
            _videoMetadataSearchingRepository = repository;
        }


        public async Task<List<VideoMetadata>>  searchVideoMetadataByCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentException("Category name cannot be empty or null.", nameof(categoryName));
            }
            // Delegate the call to the injected repository
            return await _videoMetadataSearchingRepository.searchVideoMetadataByCategory(categoryName);
        }

        public async Task<List<VideoMetadata>> searchVideoMetadataByDescription(string videoDescription)
        {
            if (string.IsNullOrWhiteSpace(videoDescription))
            {
                throw new ArgumentException("Video description cannot be empty or null.", nameof(videoDescription));
            }
            // Delegate the call to the injected repository
            return await _videoMetadataSearchingRepository.searchVideoMetadataByDescription(videoDescription);
        }

        public async Task<List<VideoMetadata>> searchVideoMetadataByName(string videoName)
        {
            if (string.IsNullOrWhiteSpace(videoName))
            {
                throw new ArgumentException("Video name cannot be empty or null.", nameof(videoName));
            }
            // Delegate the call to the injected repository
            return await _videoMetadataSearchingRepository.searchVideoMetadataByName(videoName);
        }

        public async Task<List<VideoMetadata>> searchVideoMetadataByTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                throw new ArgumentException("Tag name cannot be empty or null.", nameof(tagName));
            }
            // Delegate the call to the injected repository
            return await _videoMetadataSearchingRepository.searchVideoMetadataByTag(tagName);
        }
    }
}
