using RabbitMQ.Client;

namespace Backend.Services.RabbitMq.Interfaces
{
    public interface IRabbitMqConnection
    {
        IConnection connection { get; }
    }
}
