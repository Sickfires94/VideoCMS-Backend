namespace Backend.DTOs.ResponseDtos
{
    // Backend/DTOs/CategoryResponseDto.cs
    // For single category responses (e.g., Create, GetById, GetByName)
    public class CategoryResponseDto
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public int? parentCategoryId { get; set; } // Optional: for display if available
    }

    // Backend/DTOs/CategoryTreeItemDto.cs
    // For tree-like responses (e.g., GetImmediateChildren, GetDescendantCategories, GetAllCategoriesWithChildren)
    // This DTO is self-referencing for the 'children' collection, but critically omits 'CategoryParent'
    // to prevent the cyclic reference in the upwards direction.
    public class CategoryTreeItemDto
    {
        public int categoryId { get; set; }
        public string CategoryName { get; set; }
        public List<CategoryTreeItemDto>? Children { get; set; }
    }

    public class CategoryTreeResponseDto
    {
        public IEnumerable<CategoryTreeItemDto> categories { get; set; }
    }

}
