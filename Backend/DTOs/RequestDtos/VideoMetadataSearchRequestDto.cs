namespace Backend.DTOs.RequestDtos;

public class VideoMetadataSearchRequestDto
{
    public string? searchTerm { get; set; }
    public int? categoryId { get; set; }

    // Pagination parameters
    public int pageNumber { get; set; } = 1; // Default to page 1
    public int pageSize { get; set; } = 20;  // Default to 10 items per page


}