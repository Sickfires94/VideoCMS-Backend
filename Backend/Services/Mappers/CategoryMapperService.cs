using Backend.DTOs;
using Backend.DTOs.RequestDtos;
using Backend.DTOs.ResponseDtos.Categories;
using Backend.Services.Mappers.Interfaces;

namespace Backend.Services.Mappers
{
    public class CategoryMapperService : ICategoryMapperService
    {
        public Category toEntity(CategoryRequestDto request)
        {
            return new Category
            {
                categoryName = request.categoryName,
                categoryParentId = request.parentCategoryId
            };
        }

        public CategoryResponseDto toResponse(Category entity)
        {
            return new CategoryResponseDto 
            { 
                categoryId = entity.categoryId,
                categoryName = entity.categoryName,
                parentCategoryId = entity.categoryParentId,
            };
        }

        public CategoryTreeItemDto toTree()
        {
            throw new NotImplementedException();
        }
    }
}
