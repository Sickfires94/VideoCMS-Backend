using Backend.Services.RabbitMq.Enums;
using Backend.Services.RabbitMq.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace Backend.Services.RabbitMq
{
    public abstract class BaseRabbitMqConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private IChannel _channel;
        private AsyncEventingBasicConsumer _consumer;
        private string _consumerTag;

        protected abstract string QueueName { get; }

        protected abstract Task<ProcessResult> ProcessMessage(
            string message,
            string routingKey
        );

        public BaseRabbitMqConsumer(IRabbitMqConnection Connection)
        {
            _connection = Connection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _channel = await _connection.connection.CreateChannelAsync();

                _consumer = new AsyncEventingBasicConsumer(_channel);

                _consumer.ReceivedAsync += async (model, ea) =>
                {
                    var messageContent = Encoding.UTF8.GetString(ea.Body.ToArray());

                    Debug.WriteLine($"Received message: {messageContent} with routing key: {ea.RoutingKey}");

                    ProcessResult result;

                    try
                    {
                        result = await ProcessMessage(messageContent, ea.RoutingKey);
                    }

                    catch (Exception ex)
                    {
                        result = ProcessResult.FailRequeue;
                    }

                    switch (result)
                    {
                        case ProcessResult.Success:
                            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                            break;
                        case ProcessResult.FailDiscard:
                            await _channel.BasicRejectAsync(ea.DeliveryTag, requeue: false); // Discard without re-queue
                            break;
                        case ProcessResult.FailRequeue:
                        default:
                            await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true); // Re-queue
                            break;
                    }
                };

                     _consumerTag = await _channel.BasicConsumeAsync(
                        queue: QueueName,
                        autoAck: false, // We handle acknowledgments manually
                        consumer: _consumer
                    );

                     await Task.Delay(Timeout.Infinite, stoppingToken);
                 }

                 catch (Exception ex)
            {
                /// Implement log here
                throw;
            }

            finally
            {
                StopRabbitMqConsumer();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            StopRabbitMqConsumer();
            return base.StopAsync(cancellationToken); // Call base implementation
        }

        private void StopRabbitMqConsumer()
        {
            if (_channel != null && _channel.IsOpen)
            {
                if (!string.IsNullOrEmpty(_consumerTag))
                {
                    try
                    {
                       _channel.BasicCancelAsync(_consumerTag); // Cancel the consumer
                    }
                    catch (Exception)
                    {
                        // Ignore exceptions during cancel on shutdown
                    }
                }
                _channel.CloseAsync(); // Close the channel
                _channel.Dispose(); // Dispose the channel
            }
            _channel = null; // Clear the reference
        }

    }
}
