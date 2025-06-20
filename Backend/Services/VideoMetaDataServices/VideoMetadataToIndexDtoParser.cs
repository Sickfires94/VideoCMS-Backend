using Backend.DTOs;
using Backend.Services.VideoMetaDataServices.Interfaces;

namespace Backend.Services.VideoMetaDataServices
{
    public class VideoMetadataToIndexDtoParser : IVideoMetadataToIndexDtoParser
    {
        public VideoMetadataIndexDTO parseVideoMetadataToIndex(VideoMetadata videoMetadata)
        {
                       if (videoMetadata == null)
            {
                throw new ArgumentNullException(nameof(videoMetadata), "Video metadata cannot be null for indexing.");
            }
            // Create a new VideoMetadataIndexDTO from the VideoMetadata object
            return new VideoMetadataIndexDTO(
                videoMetadata.videoId,
                videoMetadata.videoName,
                videoMetadata.videoDescription ?? "",
                videoMetadata.videoUrl,
                videoMetadata.videoTags?.Select(tag => tag.tagName).ToList() ?? new List<string>(),
                videoMetadata.category?.categoryName ?? "",
                videoMetadata.user.userName,
                videoMetadata.videoUploadDate,
                videoMetadata.videoUpdatedDate
            );
        }
    }
}
