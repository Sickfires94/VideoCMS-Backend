using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.RequestDtos
{
    public class VideoMetadataRequestDto
    {

        public string videoName { get; set; }
        public string? videoDescription { get; set; }

    
        [Url]
        public string? videoUrl { get; set; }


        public IEnumerable<TagRequestDto> videoTags { get; set; } = new List<TagRequestDto>()!;

        public int categoryId{ get; set; }
    }
}
