using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos;

namespace Backend.Services.Mappers.Interfaces
{
    public interface ICategoryMapperService
    {
        public Category toEntity(CategoryRequestDto request);
        public CategoryResponseDto toResponse(Category entity);

        public CategoryTreeItemDto toTree();
    }
}
