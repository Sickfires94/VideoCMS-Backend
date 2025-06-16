namespace Backend.Services.RabbitMq.Interfaces
{
    public interface IMessageConsumer
    {
        Task StartConsumingAsync(CancellationToken token);
        Task StopConsumingAsync(CancellationToken token);
    }
}
