using Backend.DTOs;
using Backend.Services.Interface;
using Backend.Services.Interfaces;
using Backend.Services.VideoMetaDataServices.Interfaces;

namespace Backend.Services.VideoMetaDataServices
{
    public class PopulateVideoMetadataService : IPopulateVideoMetadataService
    {

        private readonly ILogger<PopulateVideoMetadataService> _logger;
        private readonly ITagService _tagService;
        private readonly ICategoryService _categoryService;

        public PopulateVideoMetadataService(ILogger<PopulateVideoMetadataService> logger, ITagService tagService, ICategoryService categoryService)
        {
            _logger = logger;
            _tagService = tagService;
            _categoryService = categoryService;
        }

        public async Task<VideoMetadata> populate(VideoMetadata videoMetadata)
        {
            if (videoMetadata == null) return null;

            if (videoMetadata.categoryId.HasValue)
            {
                videoMetadata.category = await _categoryService.GetCategoryByIdAsync(videoMetadata.categoryId.Value);
            }

            if(videoMetadata.videoTags != null & videoMetadata.videoTags.Count > 0)
            {
                ICollection<Tag> tags = new List<Tag>();

                foreach (Tag tag in videoMetadata.videoTags) 
                {
                    tags.Append(await _tagService.GetOrCreateTagByName(tag.tagName));
                }

                videoMetadata.videoTags = tags;
            }

            return videoMetadata;
        }
    }
}
