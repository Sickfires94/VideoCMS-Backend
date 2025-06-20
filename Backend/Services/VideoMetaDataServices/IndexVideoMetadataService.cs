using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;

namespace Backend.Services.VideoMetaDataServices
{
    public class IndexVideoMetadataService : IIndexVideoMetadataService
    {

        private readonly IIndexVideoMetadataRepository _indexvideoMetadataRepository;
        private readonly IVideoMetadataToIndexDtoParser _videoMetadataToIndexDtoParser;



        public IndexVideoMetadataService(IIndexVideoMetadataRepository indexvideoMetadataRepository, IVideoMetadataToIndexDtoParser videoMetadataToIndexDtoParser)
        {
            Console.WriteLine("***********************************");
            Console.WriteLine("Initializing Index Video Service");
            _indexvideoMetadataRepository = indexvideoMetadataRepository;
            _videoMetadataToIndexDtoParser = videoMetadataToIndexDtoParser;
        }


        public async void bulkIndexVideoMetadata(IEnumerable<VideoMetadata> videoMetadatas)
        {
            if (videoMetadatas == null || !videoMetadatas.Any())
            {
                Console.WriteLine("Service: No video metadata provided for bulk indexing. Skipping operation.");
                return;
            }

            await _indexvideoMetadataRepository.bulkIndexVideoMetadata(videoMetadatas);
            Console.WriteLine($"Service: Bulk indexed {videoMetadatas.Count()} video metadata documents.");
        }

        public async void deleteVideoMetadataFromIndex(VideoMetadata videoMetadata)
        {
            if (videoMetadata == null)
            {
                throw new ArgumentNullException(nameof(videoMetadata), "Video metadata cannot be null for deletion.");
            }
            if (videoMetadata.videoId <= 0)
            {
                throw new ArgumentException("Video ID must be valid for deletion.", nameof(videoMetadata.videoId));
            }

            // The service takes the DTO, but the repository expects the ID
            await _indexvideoMetadataRepository.deleteVideoMetadataFromIndex(videoMetadata.videoId);
            Console.WriteLine($"Service: Deleted video metadata with ID {videoMetadata.videoId} from index.");
        }

        public async Task indexVideoMetadata(VideoMetadata videoMetadata)
        {
            if (videoMetadata == null)
            {
                throw new ArgumentNullException(nameof(videoMetadata), "Video metadata cannot be null for indexing.");
            }

            VideoMetadataIndexDTO videoMetadataIndexDto = _videoMetadataToIndexDtoParser.parseVideoMetadataToIndex(videoMetadata);

            await _indexvideoMetadataRepository.indexVideoMetadata(videoMetadataIndexDto);
            Console.WriteLine($"Service: Indexed video metadata with ID {videoMetadata.videoId}.");

        }
    }
}
