namespace Backend.DTOs;

public class VideoSearchRequestDto
{
    public string? SearchTerm { get; set; }
    public string? CategoryId { get; set; } // Assuming CategoryId as string for direct match
    public List<string>? TagIds { get; set; } // List of tag IDs for filtering

    // Sorting parameters
    public string? SortBy { get; set; } // e.g., "uploadDate", "videoName", "views"
    public string? SortDirection { get; set; } // "asc" or "desc"

    // Pagination parameters
    public int PageNumber { get; set; } = 1; // Default to page 1
    public int PageSize { get; set; } = 10;  // Default to 10 items per page
}