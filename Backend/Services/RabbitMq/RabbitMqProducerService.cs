using Backend.Contracts;
using Backend.Services.RabbitMq.Interfaces;
using Elastic.Clients.Elasticsearch.Mapping;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Backend.Services.RabbitMq
{
    public class RabbitMqProducerService : IMessageProducer
    {
        private readonly IRabbitMqConnection _connection;
    public RabbitMqProducerService(IRabbitMqConnection connection) 
        {
            _connection = connection;
        }

        public async void produceAsync(IDataSyncMessage message, string routingKey, string exchange)
        {

            using var channel = await _connection.connection.CreateChannelAsync();


            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
            };

            await channel.QueueDeclareAsync(exchange, true, false, false, null, noWait: true);
            await channel.ExchangeDeclareAsync(exchange: exchange,
                type: ExchangeType.Fanout, // <-- Define the type here (e.g., Topic, Direct, Fanout)
                durable: true,            // <-- The exchange will survive broker restarts
                autoDelete: false,        // <-- The exchange will NOT be deleted when all queues unbind from it
                arguments: null);


            await channel.QueueBindAsync(
                queue: exchange,        // The name of the queue you declared
                exchange: exchange,     // The name of the exchange you declared
                routingKey: routingKey, // The routing key for THIS binding.
                arguments: null,
                noWait: true
);

            await channel.BasicPublishAsync(exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);



        }

        
    }
}
