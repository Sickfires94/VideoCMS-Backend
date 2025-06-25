using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.DTOs
{
    public class VideoMetadataChangeLog
    {
        [Key]
        [Column(Order = 0)]
        public int VideoId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime ChangeTime { get; set; } = DateTime.UtcNow; // Set default to UTC now

        [Required]
        [MaxLength(20)] // e.g., "Insert", "Update", "Delete"
        public string ChangeType { get; set; }

        public string? PreviousVideoName { get; set; }
        public string? UpdatedVideoName { get; set; }
        public string? PreviousVideoDescription { get; set; }
        public string? UpdatedVideoDescription { get; set; }
        public string? PreviousVideoUrl { get; set; } // Assuming VideoMetadata has a VideoUrl
        public string? UpdatedVideoUrl { get; set; } // Assuming VideoMetadata has a VideoUrl
        public int? PreviousCategoryId { get; set; }
        public int? UpdatedCategoryId { get; set; }
        // public int? UpdatedBy { get; set; } // Foreign key to User, or just store User ID
        
        public string? UpdatedByUserName { get; set; } // The name of the user who made the change

    }
}
