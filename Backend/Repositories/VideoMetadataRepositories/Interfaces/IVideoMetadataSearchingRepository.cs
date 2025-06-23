using Backend.DTOs;

namespace Backend.Repositories.VideoMetadataRepositories.Interfaces;

public interface IVideoMetadataSearchingRepository
{

    // NEW: Simpler search by a single string query across multiple fields
    Task<List<VideoMetadataIndexDTO>> SearchByGeneralQueryAsync(string query);
    Task<bool> CreateIndexWithOnlyDynamicStringMappingAsync(string query);

    public Task<List<string>> GetSuggestionsAsync(string query);
}