namespace Backend.DTOs
{
    public class VideoMetadata
    {
        public int videoId { get; set; }
        public string videoName { get; set; }
        public string videoDescription { get; set; }
        public string videoUrl { get; set; }
        public ICollection<Tag>? videoTags { get; set; }
        public Category? category { get; set; }

        public User user { get; set; }

        public DateOnly videoUploadDate { get; set; }
        public DateOnly videoUpdatedDate { get; set; }
    }
}
