using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.ResponseDtos
{
    public class VideoMetadataResponseDto
    {   
        public int videoId { get; set; }
        public string videoName { get; set; }
        public string? videoDescription { get; set; } = "";

        public string videoUrl { get; set; }

        public ICollection<TagResponseDto>? videoTags { get; set; } = new List<TagResponseDto>()!;
        public CategoryResponseDto? category { get; set; }
        public string userName { get; set; }
        public DateTime videoUploadDate { get; set; }
    }
}
