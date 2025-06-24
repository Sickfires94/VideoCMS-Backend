namespace Backend.DTOs
{
    // Backend/DTOs/CategoryResponseDto.cs
    // For single category responses (e.g., Create, GetById, GetByName)
    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? CategoryParentId { get; set; }
        public string? ParentCategoryName { get; set; } // Optional: for display if available
    }

    // Backend/DTOs/CategoryTreeItemDto.cs
    // For tree-like responses (e.g., GetImmediateChildren, GetDescendantCategories, GetAllCategoriesWithChildren)
    // This DTO is self-referencing for the 'children' collection, but critically omits 'CategoryParent'
    // to prevent the cyclic reference in the upwards direction.
    public class CategoryTreeItemDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? CategoryParentId { get; set; }
        public string? ParentCategoryName { get; set; } // For convenience
        public List<CategoryTreeItemDto>? Children { get; set; }
    }

    // For methods that return a list of categories where parents/children are NOT needed:
    // This can be the same as CategoryResponseDto, or even simpler if needed.
    public class CategoryListDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? CategoryParentId { get; set; }
    }

}
