using AutoMapper;
using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services.RabbitMq;
using Backend.Services.VideoMetaDataServices.Interfaces;

namespace Backend.Services.VideoMetaDataServices
{
    public class VideoMetadataService : IVideoMetadataService
    {
        private readonly IVideoMetadataRepository _videoMetadataRepository;
        private readonly IVideoMetaDataProducerService _producerService;


        public VideoMetadataService(IVideoMetadataRepository videoMetadataRepository, IVideoMetaDataProducerService videoMetadataProducerService, IMapper mapper)
        {
            _videoMetadataRepository = videoMetadataRepository;
            _producerService = videoMetadataProducerService;

        }
        public async Task<VideoMetadata> addVideoMetadata(VideoMetadata videoMetadata)
        {
            if (videoMetadata == null)
            {
                throw new ArgumentNullException(nameof(videoMetadata), "Video metadata cannot be null.");
            }



            Console.WriteLine($"Service: Added video metadata for videoId: {videoMetadata.videoId}.");

            VideoMetadata video = await _videoMetadataRepository.addVideoMetadata(videoMetadata);
     

            _producerService.publishVideoMetaDataAsync(videoMetadata);
            return video;
        }

        public async Task<List<VideoMetadata>> getAllVideoMetadata()
        {
            var videoMetadatas = await _videoMetadataRepository.getAllVideoMetadata();
            Console.WriteLine($"Service: Retrieved {videoMetadatas.Count} video metadata records.");
            return videoMetadatas;
        }

        public async Task<VideoMetadata> getVideoMetadataById(int id)
        {
            if (id <= 0) // Basic ID validation
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Video ID must be a positive integer.");
            }

            var videoMetadata = await _videoMetadataRepository.getVideoMetadataById(id);
            if (videoMetadata != null)
            {
                Console.WriteLine($"Service: Retrieved video metadata for videoId: {id}.");
            }
            else
            {
                Console.WriteLine($"Service: Video metadata for videoId: {id} not found.");
            }
            return videoMetadata;
        }
    }
}
