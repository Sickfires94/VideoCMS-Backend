using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IVideoMetaDataProducerService
    {
        void publishVideoMetaDataAsync(VideoMetadata video);
    }
}
