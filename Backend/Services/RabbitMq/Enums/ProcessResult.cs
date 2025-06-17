namespace Backend.Services.RabbitMq.Enums
{
    public enum ProcessResult
    {
        Success,
        FailRequeue,
        FailDiscard
    }
}
