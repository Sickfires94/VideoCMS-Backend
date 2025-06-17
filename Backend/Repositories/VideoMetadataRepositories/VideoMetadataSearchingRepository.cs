using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;

namespace Backend.Repositories.VideoMetadataRepositories
{
    public class VideoMetadataSearchingRepository : IVideoMetadataSearchingRepository
    {

        private readonly ElasticsearchClient _client;
        private const string IndexName = "videometadata"; // Define your Elasticsearch index name

        public VideoMetadataSearchingRepository(ElasticsearchClient client)
        {
            _client = client;
        }


        // IVideoMetadataSearchRepository implementations

        public async Task<List<VideoMetadata>> searchVideoMetadataByName(string videoName)
        {
            var searchResponse = _client.Search<VideoMetadata>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.videoName)
                        .Query(videoName).Fuzziness("AUTO")
                    )
                )
                .Size(20) // We expect only one or the first match
            );

            if (!searchResponse.IsSuccess())
            {
                Console.WriteLine($"Error searching video metadata by name '{videoName}': {searchResponse.DebugInformation}");
                return null;
            }

            return searchResponse.Documents.ToList();
        }

        public async Task<List<VideoMetadata>> searchVideoMetadataByDescription(string videoDescription)
        {
            var searchResponse = _client.Search<VideoMetadata>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.videoDescription)
                        .Query(videoDescription).Fuzziness("AUTO")
                    )
                )
                .Size(20)
            );

            if (!searchResponse.IsSuccess())
            {
                Console.WriteLine($"Error searching video metadata by description '{videoDescription}': {searchResponse.DebugInformation}");
                return null;
            }

            return searchResponse.Documents.ToList();
        }

        public async Task<List<VideoMetadata>> searchVideoMetadataByCategory(string categoryName)
        {
            // Assuming 'category' is mapped as an object with 'categoryName' field in Elasticsearch
            var searchResponse = _client.Search<VideoMetadata>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.category.categoryName) // Access nested property
                        .Query(categoryName).Fuzziness("AUTO")
                    )
                )
                .Size(20)
            );

            if (!searchResponse.IsSuccess())
            {
                Console.WriteLine($"Error searching video metadata by category '{categoryName}': {searchResponse.DebugInformation}");
                return null;
            }

            return searchResponse.Documents.ToList();
        }

        public async Task<List<VideoMetadata>> searchVideoMetadataByTag(string tagName)
        {
            // Assuming 'videoTags' is mapped as a nested field or an array of objects
            // and 'tagName' is a field within each tag object.
            // For nested objects, you might need a nested query, but for simple arrays of objects
            // where you're just matching a field, a direct match on the field path often works.
            var searchResponse = _client.Search<VideoMetadata>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.videoTags.Select(t => t.tagName).First()) // Access property within collection
                        .Query(tagName).Fuzziness("AUTO")
                    )
                )
                .Size(20)
            );

            if (!searchResponse.IsSuccess())
            {
                Console.WriteLine($"Error searching video metadata by tag '{tagName}': {searchResponse.DebugInformation}");
                return null;
            }

            return searchResponse.Documents.ToList();
        }
    }
}
