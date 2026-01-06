using EmailWorker.Contract;
using RabbitMQ.Client;

namespace EmailWorker.Service
{
    public sealed class RabbitMqHostedService : IHostedService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly ConnectionFactory _factory;

        public RabbitMqHostedService(IRabbitMqConnection connection)
        {
            _connection = connection;
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var conn = await _factory.CreateConnectionAsync(cancellationToken);
            _connection.SetConnection(conn);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _connection.Connection?.Dispose();
            return Task.CompletedTask;
        }
    }

}