using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services.Interfaces;
using Backend.Services.RabbitMq;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend.Services.VideoMetaDataServices
{
    public class VideoMetadataService : IVideoMetadataService
    {
        private readonly IVideoMetadataRepository _videoMetadataRepository;
        private readonly IVideoMetaDataProducerService _producerService;
        private readonly ILogger<VideoMetadataService> _logger;
        private readonly IVideoMetadata_changeLogService _videoMetadataChangeLogService;
        private readonly IPopulateVideoMetadataService _populateVideoMetadataService;
        private readonly ITokenClaimsAccessor _tokenClaimsAccessor;


        public VideoMetadataService(
            IVideoMetadataRepository videoMetadataRepository, 
            IVideoMetaDataProducerService videoMetadataProducerService,
            ILogger<VideoMetadataService> logger,
            IVideoMetadata_changeLogService videoMetadata_changeLogService,
            IPopulateVideoMetadataService populateVideoMetadataService,
            ITokenClaimsAccessor tokenClaimsAccessor
            )
        {
            _videoMetadataRepository = videoMetadataRepository;
            _producerService = videoMetadataProducerService;
            _videoMetadataChangeLogService = videoMetadata_changeLogService;
            _logger = logger;
            _populateVideoMetadataService = populateVideoMetadataService;
            _tokenClaimsAccessor = tokenClaimsAccessor;
        }
        public async Task<VideoMetadata> addVideoMetadata(VideoMetadata videoMetadata)
        {
            if (videoMetadata == null)
            {
                throw new ArgumentNullException(nameof(videoMetadata), "Video metadata cannot be null.");
            }

            _logger.LogInformation($"Service: Added video metadata for videoId: {videoMetadata.videoId}.");

            VideoMetadata video = await _populateVideoMetadataService.populate(videoMetadata);
            
            var userId = _tokenClaimsAccessor.getLoggedInUserId();

            if (!userId.HasValue) throw new UnauthorizedAccessException(); // throw exception to catch in Controller

            video.userId = userId.Value;



            await _videoMetadataRepository.addVideoMetadata(video);


            await _producerService.publishVideoMetaDataAsync(video);
            return video;
        }

        public async Task deleteVideoMetadata(int id)
        {

            if (!await IsOwnerAsync(id))
                throw new UnauthorizedAccessException("You do not have permission to delete this video.");

            await _videoMetadataRepository.deleteVideoMetadata(id);
            
            /// TODO Replace with Consumer
            // await _indexVideoMetadataRepository.deleteVideoMetadataFromIndex(id); replace with consumer
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

        public async Task<VideoMetadata> updateVideoMetadata(int id, VideoMetadata video)
        {

            if (!await IsOwnerAsync(id))
                throw new UnauthorizedAccessException("You do not have permission to delete this video.");

            VideoMetadata updatedVideo = await _populateVideoMetadataService.populate(video);

            await _videoMetadataRepository.updateVideoMetadata(id, video);
            Debug.WriteLine("user as service: " + updatedVideo.user.userName);
            return updatedVideo;
        }

        private async Task<bool> IsOwnerAsync(int videoId)
        {
            var video = await _videoMetadataRepository.getVideoMetadataById(videoId);
            var currentUserId = _tokenClaimsAccessor.getLoggedInUserId();


            if (video == null || currentUserId.HasValue)
                return false;

            return video.userId == currentUserId.Value; ;
        }
    }
}
