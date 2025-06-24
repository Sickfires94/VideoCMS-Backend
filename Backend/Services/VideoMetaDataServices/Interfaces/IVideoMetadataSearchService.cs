using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IVideoMetadataSearchService
    {

        public Task<List<VideoMetadataIndexDTO>> SearchVideoMetadata(string query);
        public Task<List<VideoMetadataIndexDTO>> SearchVideoMetadataWithCategory(string query, string categoryName);
        Task<IReadOnlyCollection<string>> GetSuggestionsAsync(string query);

    }
}
