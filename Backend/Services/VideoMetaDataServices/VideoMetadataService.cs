using Backend.DTOs;
using Backend.Repositories.VideoMetadataRepositories.Interfaces;
using Backend.Services.Interfaces;
using Backend.Services.RabbitMq;
using Backend.Services.VideoMetaDataServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend.Services.VideoMetaDataServices
{
    public class VideoMetadataService : IVideoMetadataService
    {
        private readonly IVideoMetadataRepository _videoMetadataRepository;
        private readonly IVideoMetaDataProducerService _producerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<VideoMetadataService> _logger;
        private readonly IVideoMetadata_changeLogService _videoMetadataChangeLogService;


        public VideoMetadataService(
            IVideoMetadataRepository videoMetadataRepository, 
            IVideoMetaDataProducerService videoMetadataProducerService, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger<VideoMetadataService> logger, 
            IVideoMetadata_changeLogService videoMetadata_changeLogService)
        {
            _videoMetadataRepository = videoMetadataRepository;
            _producerService = videoMetadataProducerService;
            _httpContextAccessor = httpContextAccessor;
            _videoMetadataChangeLogService = videoMetadata_changeLogService;
            _logger = logger;
        }
        public async Task<VideoMetadata> addVideoMetadata(VideoMetadata videoMetadata)
        {
            if (videoMetadata == null)
            {
                throw new ArgumentNullException(nameof(videoMetadata), "Video metadata cannot be null.");
            }



            Console.WriteLine($"Service: Added video metadata for videoId: {videoMetadata.videoId}.");

            VideoMetadata video = await _videoMetadataRepository.addVideoMetadata(videoMetadata);

            Debug.WriteLine("Video Id to publish " + video.videoId);

            await _producerService.publishVideoMetaDataAsync(video);
            return video;
        }

        public async Task deleteVideoMetadata(int id)
        {

            if (!await IsOwnerAsync(id))
                throw new UnauthorizedAccessException("You do not have permission to delete this video.");

            await _videoMetadataRepository.deleteVideoMetadata(id);
           //  await _indexVideoMetadataRepository.deleteVideoMetadataFromIndex(id); replace with consumer
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

            VideoMetadata updatedVideo = await _videoMetadataRepository.updateVideoMetadata(id, video);
            Debug.WriteLine("user as service: " + updatedVideo.user.userName);
            return updatedVideo;
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }

        private async Task<bool> IsOwnerAsync(int videoId)
        {
            var video = await _videoMetadataRepository.getVideoMetadataById(videoId);
            var currentUserId = GetCurrentUserId();

            if (video == null || string.IsNullOrEmpty(currentUserId))
                return false;

            return video.userId == Int32.Parse(currentUserId); ;
        }
    }
}
