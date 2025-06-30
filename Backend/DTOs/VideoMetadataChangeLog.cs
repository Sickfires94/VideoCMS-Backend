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
        public string? PreviousVideoUrl { get; set; }
        public string? UpdatedVideoUrl { get; set; } 
        public int? PreviousCategoryId { get; set; }
        public int? UpdatedCategoryId { get; set; }
        
        public string? UpdatedByUserName { get; set; }

    }
}
