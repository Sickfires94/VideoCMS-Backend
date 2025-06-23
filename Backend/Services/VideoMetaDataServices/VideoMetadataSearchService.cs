using Backend.Configurations.DataConfigs;
using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces; // Corrected using statement
using Backend.Services.VideoMetaDataServices.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks; // Ensure this is present for Task

namespace Backend.Services.VideoMetaDataServices
{
    public class VideoMetadataSearchService : IVideoMetadataSearchService
    {
        private readonly IVideoMetadataSearchingRepository _videoMetadataSearchingRepository;

        public VideoMetadataSearchService(IVideoMetadataSearchingRepository repository)
        {
            _videoMetadataSearchingRepository = repository;
        }


        public async Task<IReadOnlyCollection<string>> GetSuggestionsAsync(string query)
        {
            return await _videoMetadataSearchingRepository.GetSuggestionsAsync(query);
        }

        public async Task<List<VideoMetadataIndexDTO>> SearchVideoMetadata(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Return an empty list if the query is empty or just whitespace.
                // This is generally better than throwing an exception for an empty search.
                return new List<VideoMetadataIndexDTO>();
            }

            // Delegate the generic search to the repository
            var results = await _videoMetadataSearchingRepository.SearchByGeneralQueryAsync(query);

            // The repository already returns List<VideoMetadata>, which implements ICollection<VideoMetadata>
            return results;
        }
    }
}