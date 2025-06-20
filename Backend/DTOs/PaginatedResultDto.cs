namespace Backend.DTOs;

public class PaginatedResultDto<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public long TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}