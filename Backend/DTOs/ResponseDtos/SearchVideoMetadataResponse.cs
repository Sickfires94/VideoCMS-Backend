namespace Backend.DTOs.ResponseDtos
{
    public class SearchVideoMetadataResponse
    {
        public ICollection<VideoMetadataIndexDTO> videos { get; set; } = new List<VideoMetadataIndexDTO>();

    }
}
