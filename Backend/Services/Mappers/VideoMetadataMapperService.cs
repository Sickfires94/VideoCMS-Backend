using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;
using Backend.DTOs.ResponseDtos.Categories;
using Backend.Services.Mappers.Interfaces;
using System.Diagnostics;

namespace Backend.Services.Mappers
{
    public class VideoMetadataMapperService : IVideoMetadataMapperService
    {
        private readonly ICategoryMapperService _categoryMapperService;
        private readonly ITagMapperService _tagMapperService;

       public VideoMetadata ToEntity(VideoMetadataRequestDto request)
        {
            return new VideoMetadata { 
                videoName = request.videoName,
                videoDescription = request.videoDescription,
                videoUrl = request.videoUrl,
                videoTags = request.videoTags.Select(t => _tagMapperService.toEntity(t)).ToList(),
                categoryId = request.categoryId,
            };
        }

        
        public VideoMetadataResponseDto ToResponse(VideoMetadata entity)
        {
            CategoryResponseDto category = null;
            List<TagResponseDto> tags = new List<TagResponseDto>();
            string userName = entity.user?.userName ?? "";

            Debug.WriteLine("Category Id: " + entity.categoryId);
            Debug.WriteLine("Category Name: " + entity.category);

            if (entity.category != null) 
                category = _categoryMapperService.toResponse(entity.category);

            if (entity.videoTags != null && entity.videoTags.Count > 0)
                tags = entity.videoTags.Select(t => _tagMapperService.toResponse(t)).ToList();


            VideoMetadataResponseDto response = new VideoMetadataResponseDto
            {
                videoId = entity.videoId,
                videoName = entity.videoName,
                videoDescription = entity.videoDescription,
                videoUrl = entity.videoUrl,
                videoTags = tags,
                category =category,
                userName = userName
            };



            return response;
        }

        
    }
}
