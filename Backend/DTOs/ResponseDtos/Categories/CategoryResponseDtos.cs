namespace Backend.DTOs.ResponseDtos.Categories
{
    // Backend/DTOs/CategoryResponseDto.cs
    // For single category responses (e.g., Create, GetById, GetByName)
    public class CategoryResponseDto
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public int? parentCategoryId { get; set; }
    }

   
}
