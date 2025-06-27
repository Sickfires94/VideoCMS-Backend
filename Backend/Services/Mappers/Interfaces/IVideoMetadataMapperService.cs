using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;

namespace Backend.Services.Mappers.Interfaces
{
    public interface IVideoMetadataMapperService 
    {
        public VideoMetadata ToEntity(VideoMetadataRequestDto request);
        public VideoMetadataResponseDto ToResponse(VideoMetadata entity);
    }
}
