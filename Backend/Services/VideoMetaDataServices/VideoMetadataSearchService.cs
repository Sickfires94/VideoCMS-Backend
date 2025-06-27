using Backend.Configurations.DataConfigs;
using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces; // Corrected using statement
using Backend.Services.Interfaces;
using Backend.Services.VideoMetaDataServices.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks; // Ensure this is present for Task

namespace Backend.Services.VideoMetaDataServices
{
    public class VideoMetadataSearchService : IVideoMetadataSearchService
    {
        private readonly IVideoMetadataSearchingRepository _videoMetadataSearchingRepository;
        private readonly ICategoryService _categoryService;

        public VideoMetadataSearchService(IVideoMetadataSearchingRepository repository, ICategoryService categoryService)
        {
            _videoMetadataSearchingRepository = repository;
            _categoryService = categoryService;
        }


        public async Task<IReadOnlyCollection<string>> GetSuggestionsAsync(string query)
        {
            return await _videoMetadataSearchingRepository.GetSuggestionsAsync(query);
        }

        public async Task<List<VideoMetadataIndexDTO>> SearchVideoMetadataWithCategory(string query, string categoryName)
        {
            Category category = await _categoryService.GetCategoryByNameAsync(categoryName);

            if (category == null) return await _videoMetadataSearchingRepository.SearchByGeneralQueryAsync(query, null);

            List<string> categories = new List<string>();
            categories = (await _categoryService.GetDescendantCategoriesAsync(category.categoryId)).Select(c => c.categoryName).ToList();

            // Delegate the generic search to the repository
            var results = await _videoMetadataSearchingRepository.SearchByGeneralQueryAsync(query, categories);

            // The repository already returns List<VideoMetadata>, which implements ICollection<VideoMetadata>
            return results;
        }

        public async Task<List<VideoMetadataIndexDTO>> SearchVideoMetadata(string query)
        {
            // Delegate the generic search to the repository
            var results = await _videoMetadataSearchingRepository.SearchByGeneralQueryAsync(query, null);

            // The repository already returns List<VideoMetadata>, which implements ICollection<VideoMetadata>
            return results;
        }
    }
}