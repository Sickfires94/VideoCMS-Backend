
using Elastic.Transport;

namespace Backend.Contracts
{
    public class VideoMetadataSyncMessage : IDataSyncMessage
    {
        public string entityType { get; set; }
        public string payloadJson { get; set; }
        public long timestamp { get; set; }

        public VideoMetadataSyncMessage(string payloadJson)
        {
            entityType = "VideoMetadata";
            this.payloadJson = payloadJson;
            timestamp = DateTime.Now.ToFileTime();

        }
    }
}
