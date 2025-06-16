using Backend.Contracts;

namespace Backend.Services.RabbitMq.Interfaces
{
    public interface IMessageProducer
    {
        void produceAsync(IDataSyncMessage message, string routingKey, string exchange);
    }
}
