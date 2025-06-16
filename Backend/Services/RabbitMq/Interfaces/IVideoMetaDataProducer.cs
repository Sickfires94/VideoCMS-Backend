using Backend.DTOs;

namespace Backend.Services.RabbitMq.Interfaces
{
    public interface IVideoMetaDataProducer
    {
        void publishVideoMetaDataAsync(VideoMetadata video);
    }
}
