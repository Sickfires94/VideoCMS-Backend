using Backend.Configurations.DataConfigs;
using Backend.Services.RabbitMq.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Backend.Services.RabbitMq
{
    public class RabbitMqTopologyInitializer : IHostedService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly ILogger<RabbitMqTopologyInitializer> _logger;
        private readonly VideoMetadataIndexingQueueOptions _videoMetadataSettings;

        public RabbitMqTopologyInitializer(
            IRabbitMqConnection connection,
            ILogger<RabbitMqTopologyInitializer> logger,
            IOptions<VideoMetadataIndexingQueueOptions> videoMetadataOptions)
        {
            _connection = connection;
            _logger = logger;
            _videoMetadataSettings = videoMetadataOptions.Value;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var channel = await _connection.connection.CreateChannelAsync();


            await channel.QueueDeclareAsync(_videoMetadataSettings.Exchange, true, false, false, null, noWait: true);
            await channel.ExchangeDeclareAsync(exchange: _videoMetadataSettings.Exchange,
                type: ExchangeType.Fanout, // <-- Define the type here (e.g., Topic, Direct, Fanout)
                durable: true,            // <-- The exchange will survive broker restarts
                autoDelete: false,        // <-- The exchange will NOT be deleted when all queues unbind from it
                arguments: null);


            await channel.QueueBindAsync(
                queue: _videoMetadataSettings.Queue,        // The name of the queue you declared
                exchange: _videoMetadataSettings.Exchange,     // The name of the exchange you declared
                routingKey: _videoMetadataSettings.RoutingKey, // The routing key for THIS binding.
                arguments: null,
                noWait: true
                );

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


    }
}