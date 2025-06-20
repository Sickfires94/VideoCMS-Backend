using Backend.Configurations.DataConfigs;
using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Backend.Repositories.VideoMetadataRepositories
{
    public class IndexVideoMetadataRepository : IIndexVideoMetadataRepository
    {

        private readonly ElasticsearchClient _client;
        private readonly VideoMetadataIndexingOptions _videoMetadataIndexingOptions; // Define your Elasticsearch index name

        public IndexVideoMetadataRepository(ElasticsearchClient client, IOptions<VideoMetadataIndexingOptions> videoMetadataIndexingOptions)
        {
            Console.WriteLine("**************************************");
            Console.WriteLine("Initializing IndexVideoMetadataRepository with Elasticsearch client.");
            _client = client;
            _videoMetadataIndexingOptions = videoMetadataIndexingOptions.Value;

            CreateIndexAsync();
        }

        public async Task CreateIndexAsync()
        {
            var createIndexResponse = await _client.Indices.CreateAsync(_videoMetadataIndexingOptions.IndexName, c => c
                .Mappings(m => m
                    .Dynamic(DynamicMapping.True)  // Allow dynamic mapping
                )
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(1)
                )
            );

            if (!createIndexResponse.IsSuccess())
            {
                Console.WriteLine($"Failed to create index: {createIndexResponse.DebugInformation}");
            }
            else
            {
                Console.WriteLine("Index created successfully.");
            }
        }


        public async Task<VideoMetadataIndexDTO> indexVideoMetadata(VideoMetadataIndexDTO videoMetadataIndexDTO)
        {

            var response = await _client.IndexAsync(videoMetadataIndexDTO, dp => dp
                .Index(_videoMetadataIndexingOptions.IndexName)
                .Id(videoMetadataIndexDTO.videoId)
            );

            if (!response.IsSuccess())
            {
                Console.WriteLine($"Error indexing video metadata {videoMetadataIndexDTO.videoId}: {response.DebugInformation}");
                throw new Exception($"Failed to index video metadata {videoMetadataIndexDTO.videoId} to Elasticsearch: {response.DebugInformation}");
            }

            Console.WriteLine($"Successfully indexed video metadata {videoMetadataIndexDTO.videoId}.");
            // Return the document that was successfully indexed
            return videoMetadataIndexDTO;
        }

        // Changed return type from 'void' to 'Task<bool>'
        public async Task<bool> deleteVideoMetadataFromIndex(int videoId)
        {
            var response = await _client.DeleteAsync(_videoMetadataIndexingOptions.IndexName, videoId);

            if (!response.IsSuccess() && response.Result != Result.NotFound)
            {
                Console.WriteLine($"Error deleting video metadata {videoId} from Elasticsearch: {response.DebugInformation}");
                throw new Exception($"Failed to delete video metadata {videoId} from Elasticsearch: {response.DebugInformation}");
            }

            if (response.Result == Result.NotFound)
            {
                Console.WriteLine($"Video metadata {videoId} not found in Elasticsearch for deletion (already gone).");
                return false; // Indicate that it wasn't found
            }
            else // response.Result == Elastic.Clients.Elasticsearch.Result.Deleted
            {
                Console.WriteLine($"Successfully deleted video metadata {videoId}.");
                return true; // Indicate successful deletion
            }
        }

        // Changed return type from 'void' to 'Task<List<VideoMetadata>>'
        public async Task<List<VideoMetadata>> bulkIndexVideoMetadata(IEnumerable<VideoMetadata> videoMetadatas)
        {
            // Return an empty list if no items are provided
            if (videoMetadatas == null || !videoMetadatas.Any())
            {
                Console.WriteLine("No video metadata to bulk index.");
                return new List<VideoMetadata>();
            }

            var bulkResponse = await _client.BulkAsync(b => b
                .Index(_videoMetadataIndexingOptions.IndexName)
                .IndexMany(videoMetadatas, (bulkItem, video) => bulkItem.Id(video.videoId))
            );

            // Handle errors for the entire bulk request
            if (!bulkResponse.IsSuccess())
            {
                Console.WriteLine($"Error during bulk indexing: {bulkResponse.DebugInformation}");
                if (bulkResponse.Errors)
                {
                    foreach (var itemWithErrors in bulkResponse.ItemsWithErrors)
                    {
                        Console.WriteLine($"  Failed to index document {itemWithErrors.Id}: {itemWithErrors.Error?.Reason}");
                    }
                }
                throw new Exception($"Failed to bulk index video metadata to Elasticsearch: {bulkResponse.DebugInformation}");
            }

            // Filter out and return only the successfully indexed documents
            var successfullyIndexed = new List<VideoMetadata>();
            var videoMetadataList = videoMetadatas.ToList(); // Convert to list for efficient lookup

            foreach (var item in bulkResponse.Items)
            {
                if (item.Error == null)
                {
                    // Find the original document from the input collection if needed,
                    // assuming Id matches videoId.
                    var originalDoc = videoMetadataList.FirstOrDefault(v => v.videoId.ToString() == item.Id);
                    if (originalDoc != null)
                    {
                        successfullyIndexed.Add(originalDoc);
                    }
                }
            }

            Console.WriteLine($"Successfully bulk indexed {successfullyIndexed.Count} video metadata documents.");
            if (bulkResponse.ItemsWithErrors.Any())
            {
                Console.WriteLine($"Note: {bulkResponse.ItemsWithErrors.Count()} items had errors during bulk indexing.");
            }

            return successfullyIndexed;
        }

        // Add other repository methods here like SearchVideoMetadataByName etc.
    }
}

