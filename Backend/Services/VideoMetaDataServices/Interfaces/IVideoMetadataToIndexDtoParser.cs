using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IVideoMetadataToIndexDtoParser
    {
        public VideoMetadataIndexDTO parseVideoMetadataToIndex(VideoMetadata videoMetadata);
    }
}
