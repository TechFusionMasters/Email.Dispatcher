using EmailDispatcherAPI.Dto;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EmailDispatcherAPI.Service
{
    public sealed class RabbitMqHostedService : IHostedService
    {
        private readonly RabbitMqConnection _connection;
        private readonly ConnectionFactory _factory;

        public RabbitMqHostedService(RabbitMqConnection connection, IOptions<RabbitMQConfig> rabbitMQConfig)
        {
            _connection = connection;
            var config = rabbitMQConfig.Value;
            _factory = new ConnectionFactory 
            { 
                HostName = config.HostName,
                Port = config.Port,
                UserName = config.UserName,
                Password = config.Password
            };
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