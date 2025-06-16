namespace Backend.DTOs
{
    public class Category
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public Category? categoryParentId { get; set; }
    }
}
