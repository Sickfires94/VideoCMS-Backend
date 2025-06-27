using Backend.DTOs;

namespace Backend.Repositories.Interface
{
    public interface IVideoMetadata_changelogRepository
    {
        Task<IEnumerable<VideoMetadataChangeLog>> getLogsByVideoId(int videoId);
        Task LogChange(VideoMetadata video);
    }
}
