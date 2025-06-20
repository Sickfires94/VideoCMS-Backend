using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class VideoMetadata
    {

        public int videoId { get; set; }

        [Required]
        public string videoName { get; set; }
        public string? videoDescription { get; set; }

        [Required]
        // [Url]
        public string videoUrl { get; set; }


        public ICollection<Tag>? videoTags { get; set; } = new List<Tag>()!;
        
        public int? categoryId { get; set; }
        public Category? category { get; set; } = default!;

        [Required]
        public int userId { get; set; }
        public User? user { get; set; }

        public DateTime videoUploadDate { get; set; }
        public DateTime videoUpdatedDate { get; set; }
    }
}
