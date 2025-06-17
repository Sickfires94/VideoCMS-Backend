using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Elastic.Clients.Elasticsearch;
using System.Diagnostics;

namespace Backend.Repositories.VideoMetadataRepositories
{
    public class IndexVideoMetadataRepository : IIndexVideoMetadataRepository
    {

        private readonly ElasticsearchClient _client;
        private const string IndexName = "videometadata"; // Define your Elasticsearch index name

        public IndexVideoMetadataRepository(ElasticsearchClient client)
        {
            Console.WriteLine("**************************************");
            Console.WriteLine("Initializing IndexVideoMetadataRepository with Elasticsearch client.");
            _client = client;
        }


        public async Task<VideoMetadata> indexVideoMetadata(VideoMetadata videoMetadata)
        {
            Debug.WriteLine("********************************");
            Debug.WriteLine("videoMetadata: " + videoMetadata.videoId);

            var response = await _client.IndexAsync(videoMetadata, dp => dp
                .Index(IndexName)
                .Id(videoMetadata.videoId)
            );

            if (!response.IsSuccess())
            {
                Console.WriteLine($"Error indexing video metadata {videoMetadata.videoId}: {response.DebugInformation}");
                throw new Exception($"Failed to index video metadata {videoMetadata.videoId} to Elasticsearch: {response.DebugInformation}");
            }

            Console.WriteLine($"Successfully indexed video metadata {videoMetadata.videoId}.");
            // Return the document that was successfully indexed
            return videoMetadata;
        }

        // Changed return type from 'void' to 'Task<bool>'
        public async Task<bool> deleteVideoMetadataFromIndex(int videoId)
        {
            var response = await _client.DeleteAsync(IndexName, videoId);

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
                .Index(IndexName)
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

