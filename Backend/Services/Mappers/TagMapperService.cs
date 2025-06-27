using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;
using Backend.Services.Mappers.Interfaces;

namespace Backend.Services.Mappers
{
    public class TagMapperService : ITagMapperService
    {
        public Tag toEntity(TagRequestDto request)
        {
            return new Tag { 
                tagName = request.tagName,
            };
        }

        public TagResponseDto toResponse(Tag entity)
        {
            return new TagResponseDto
            {
                tagName = entity.tagName,
            };
        }

        public TagResponseDto toResponse(string name)
        {
            return new TagResponseDto { tagName = name };
        }
    }
}
