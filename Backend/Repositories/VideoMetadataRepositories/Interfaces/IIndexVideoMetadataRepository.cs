using Backend.DTOs;

namespace Backend.Repositories.VideoMetadataRepositories.Interfaces
{
    public interface IIndexVideoMetadataRepository
    {
        // Now returns the indexed VideoMetadata
        Task<VideoMetadataIndexDTO> indexVideoMetadata(VideoMetadataIndexDTO videoMetadataIndexDto);

        // Returns true if deleted, false if not found/error
        Task<bool> deleteVideoMetadataFromIndex(int videoId);

        // Returns the list of VideoMetadata that were successfully bulk indexed
        Task<List<VideoMetadata>> bulkIndexVideoMetadata(IEnumerable<VideoMetadata> videoMetadatas);
    }
}
