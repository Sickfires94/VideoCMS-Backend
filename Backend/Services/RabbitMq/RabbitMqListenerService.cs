using Backend.Services.RabbitMq.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Backend.Services.RabbitMq
{
    public class RabbitMqListenerService : IHostedService, IDisposable
    {

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

