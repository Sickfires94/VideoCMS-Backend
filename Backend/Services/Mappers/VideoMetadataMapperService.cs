using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;
using Backend.Services.Mappers.Interfaces;

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
            return new VideoMetadataResponseDto
            {
                videoId = entity.videoId,
                videoName = entity.videoName,
                videoDescription = entity.videoDescription,
                videoUrl = entity.videoUrl,
                videoTags = entity.videoTags.Select(t => _tagMapperService.toResponse(t)).ToList(),
                category =_categoryMapperService.toResponse(entity.category)
            };
        }

        
    }
}
