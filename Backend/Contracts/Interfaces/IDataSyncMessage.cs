namespace Backend.Contracts
{
    public interface IDataSyncMessage
    {
        string entityType { get; }
        string payloadJson { get; }
        long timestamp { get; }
    }
}
