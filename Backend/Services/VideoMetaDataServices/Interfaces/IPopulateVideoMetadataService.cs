using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IPopulateVideoMetadataService
    {
        public Task<VideoMetadata> populate(VideoMetadata videoMetadata); 
    }
}
