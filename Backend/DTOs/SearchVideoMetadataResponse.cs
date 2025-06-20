namespace Backend.DTOs
{
    public class SearchVideoMetadataResponse
    {
        public ICollection<VideoMetadataIndexDTO> items { get; set; } = new List<VideoMetadataIndexDTO>();

        public SearchVideoMetadataResponse(ICollection<VideoMetadataIndexDTO> items)
        {
            this.items = items;
        }

    }
}
