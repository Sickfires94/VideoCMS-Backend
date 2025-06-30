namespace Backend.DTOs.ResponseDtos.Categories
{
    public class CategoryTreeItemDto
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public List<CategoryTreeItemDto>? children { get; set; }
    }

    public class CategoryTreeResponseDto
    {
        public IEnumerable<CategoryTreeItemDto> categories { get; set; }
    }

}
