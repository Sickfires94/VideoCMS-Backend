namespace Backend.DTOs
{
    public class Category
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public int? categoryParentId { get; set; }
        public Category? categoryParent { get; set; }
        public IEnumerable<Category>? children { get; set; }
    }
}
