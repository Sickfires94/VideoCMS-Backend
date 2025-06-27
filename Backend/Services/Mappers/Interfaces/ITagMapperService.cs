using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;

namespace Backend.Services.Mappers.Interfaces
{
    public interface ITagMapperService
    {
        public Tag toEntity(TagRequestDto request);
        public TagResponseDto toResponse(Tag entity);
        public TagResponseDto toResponse(string name);
    }
}
