using Backend.DTOs;

namespace Backend.Services.VideoMetaDataServices.Interfaces
{
    public interface IIndexVideoMetadataService
    {
        public void indexVideoMetadata(VideoMetadata videoMetadata);
        public void deleteVideoMetadataFromIndex(VideoMetadata videoMetadata);

        public void bulkIndexVideoMetadata(IEnumerable<VideoMetadata> videoMetadatas);
    }
}
