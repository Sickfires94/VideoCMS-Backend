namespace Backend.DTOs.ResponseDtos
{
    public class SearchVideoMetadataResponse
    {
        public ICollection<VideoMetadataIndexDTO> items { get; set; } = new List<VideoMetadataIndexDTO>();

    }
}
