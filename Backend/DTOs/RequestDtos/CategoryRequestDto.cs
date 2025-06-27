namespace Backend.DTOs.RequestDtos
{
    public class CategoryRequestDto
    {
        public string categoryName { get; set; }
        public int? parentCategoryId { get; set; }
    }
}
