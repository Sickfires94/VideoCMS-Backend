using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs
{
    public class VideoMetadataIndexDTO
    {


        public VideoMetadataIndexDTO(int videoId, string videoName, string videoDescription, string videoUrl, ICollection<string>? videoTagNames, string categoryName, string userName, DateTime videoUploadDate, DateTime videoUpdatedDate)
        {
            this.videoId = videoId;
            this.videoName = videoName;
            this.videoDescription = videoDescription;
            this.videoUrl = videoUrl;
            this.videoTagNames = videoTagNames;
            this.categoryName = categoryName;
            this.userName = userName;
            this.videoUploadDate = videoUploadDate;
           
        }



        public int videoId { get; set; }



        [Required]
        public string videoName { get; set; }
        public string videoDescription { get; set; }

        [Required]
        [Url]
        public string videoUrl { get; set; }


        public ICollection<string>? videoTagNames { get; set; } = new List<string>()!;

        public string categoryName { get; set; } = string.Empty;

        [Required]
        public string userName { get; set; }
        
        public DateTime videoUploadDate { get; set; }
        public DateTime videoUpdatedDate { get; set; }

    }
}
