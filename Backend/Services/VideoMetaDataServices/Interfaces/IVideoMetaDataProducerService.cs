using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IVideoMetaDataProducerService
    {
        Task publishVideoMetaDataAsync(VideoMetadata video);
    }
}
