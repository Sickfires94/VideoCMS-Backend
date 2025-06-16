using Backend.Services.RabbitMq.Interfaces;
using RabbitMQ.Client;

namespace Backend.Services.RabbitMq
{
    public class RabbitMqConnection : IRabbitMqConnection, IDisposable
    {
        public IConnection connection { get; }

        public RabbitMqConnection(IConnection connection)
        {
            this.connection = connection;
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
