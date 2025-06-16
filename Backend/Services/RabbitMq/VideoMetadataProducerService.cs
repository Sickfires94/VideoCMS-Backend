using Backend.Contracts;
using Backend.DTOs;
using Backend.Services.RabbitMq.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;

namespace Backend.Services.RabbitMq
{
    public class VideoMetadataProducerService : IVideoMetaDataProducer
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string _exchange;
        private readonly string _routingKey;
        private readonly string _entityType;

        public VideoMetadataProducerService(IServiceScopeFactory scopefactory, string exchange, string routingKey, string entityType)
        {
            _scopeFactory = scopefactory;
            _exchange = exchange;
            _routingKey = routingKey;
            _entityType = entityType;
        }


        public void publishVideoMetaDataAsync(VideoMetadata video)
        {
            string videoJson = JsonSerializer.Serialize(video);
            VideoMetadataSyncMessage message = new(videoJson);

            using var scope = _scopeFactory.CreateScope();
            var genericPublisher = scope.ServiceProvider.GetRequiredService<IMessageProducer>();
            genericPublisher.produceAsync(message, _routingKey, _exchange);
        }

    }
}
