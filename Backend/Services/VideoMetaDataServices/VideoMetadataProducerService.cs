using Backend.Configurations.DataConfigs;
using Backend.Contracts;
using Backend.DTOs;
using Backend.Services.RabbitMq.Interfaces;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Backend.Services.VideoMetaDataServices
{
    public class VideoMetadataProducerService : IVideoMetaDataProducerService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string _exchange;
        private readonly string _routingKey;
        private readonly string _entityType;

        public VideoMetadataProducerService(
            IServiceScopeFactory scopeFactory,
            IOptions<VideoMetadataProducerSettings> settings) // Inject IOptions
        {
            _scopeFactory = scopeFactory;
            _exchange = settings.Value.ExchangeName; // Get from settings
            _routingKey = settings.Value.RoutingKey; // Get from settings
            _entityType = settings.Value.EntityType; // Get from settings

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
