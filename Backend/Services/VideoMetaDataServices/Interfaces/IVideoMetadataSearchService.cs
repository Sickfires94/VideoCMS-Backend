using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IVideoMetadataSearchService
    {

        public Task<List<VideoMetadataIndexDTO>> SearchVideoMetadata(string query);
        Task<IReadOnlyCollection<string>> GetSuggestionsAsync(string query);

    }
}
