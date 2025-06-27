using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IVideoMetadata_changeLogService
    {
        Task<IEnumerable<VideoMetadataChangeLog>> getLogsByVideoId(int videoId);
        Task LogChange(VideoMetadata video);
    }
}
