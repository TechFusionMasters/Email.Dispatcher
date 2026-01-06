using EmailDispatcherAPI.Contract;
using RabbitMQ.Client;

namespace EmailDispatcherAPI.Service
{
    public sealed class RabbitMqHostedService : IHostedService
    {
        private readonly RabbitMqConnection _connection;
        private readonly ConnectionFactory _factory;

        public RabbitMqHostedService(RabbitMqConnection connection)
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