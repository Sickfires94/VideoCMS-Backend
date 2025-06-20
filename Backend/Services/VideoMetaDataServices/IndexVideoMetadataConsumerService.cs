using Backend.Configurations.DataConfigs;
using Backend.Contracts;
using Backend.DTOs;
using Backend.Services.RabbitMq;
using Backend.Services.RabbitMq.Enums;
using Backend.Services.RabbitMq.Interfaces;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace Backend.Services.VideoMetaDataServices
{
    public class IndexVideoMetadataConsumerService : BaseRabbitMqConsumer
    {

        private readonly VideoMetadataIndexingQueueOptions _videoMetadataIndexingOptions; 
        private readonly IServiceScopeFactory _scopeFactory;


        public IndexVideoMetadataConsumerService(
            IRabbitMqConnection Connection,
            IServiceScopeFactory scopeFactory,
             IOptions<VideoMetadataIndexingQueueOptions> videoMetadataIndexingOptions
            ) : base(Connection)
        {


            Console.WriteLine("***********************************");
            Console.WriteLine("Consuming Data");

            _videoMetadataIndexingOptions = videoMetadataIndexingOptions.Value;
            _scopeFactory = scopeFactory;
        }

        protected override string QueueName => _videoMetadataIndexingOptions.Queue;

        protected override async Task<ProcessResult> ProcessMessage(
            string messageContent,
            string routingKey)
        {
            try
            {
                // Create a new scope for each message processing operation.
                // This ensures that IVideoMetadataService (and its DbContext)
                // is correctly scoped and disposed for each message.
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Get the IVideoMetadataService from the newly created scope
                    var indexVideoMetadataService = scope.ServiceProvider.GetRequiredService<IIndexVideoMetadataService>();

                    Console.WriteLine("***********************************");
                    Console.WriteLine("Consuming Data");

                    Debug.WriteLine("Consumer Recieved JSON: " + messageContent);


                    var videoMetadataJson = JsonSerializer.Deserialize<VideoMetadataSyncMessage>(messageContent).payloadJson;
                    var videoMetadata = JsonSerializer.Deserialize<VideoMetadata>(videoMetadataJson);

                    Debug.WriteLine("Recieved video Id" + videoMetadata.videoId);

                    if (videoMetadata == null)
                    {
                        return ProcessResult.FailDiscard;
                    }


                    await indexVideoMetadataService.indexVideoMetadata(videoMetadata);

                    return ProcessResult.Success;
                } // The scope and its services (like DbContext) are disposed here
            }
            catch (JsonException)
            {
                return ProcessResult.FailDiscard;
            }
            catch (Exception)
            {
                return ProcessResult.FailRequeue;
            }
        }
    }
}
