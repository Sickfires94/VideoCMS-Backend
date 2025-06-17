using Backend.Contracts;
using Backend.Services.RabbitMq.Interfaces;
using Elastic.Clients.Elasticsearch.Mapping;
using RabbitMQ.Client;
using System.Diagnostics;
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

            Debug.WriteLine("Sending video: " + json.ToString());

            await channel.BasicPublishAsync(exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);
        }

        
    }
}
