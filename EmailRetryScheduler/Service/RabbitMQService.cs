using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Dto;
using EmailRetryScheduler.Modal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace EmailRetryScheduler
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;

        public RabbitMQService(IOptions<RabbitMQConfig> rabbitMQConfig)
        {
            var config = rabbitMQConfig.Value;
            _factory = new ConnectionFactory 
            { 
                HostName = config.HostName,
                Port = config.Port,
                UserName = config.UserName,
                Password = config.Password
            };
        }

        public async Task CreateConnection(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
        }

        public async Task InsertMessageToRabbitMQ(EmailIdempotency emailIdempotency, string queueName)
        {
            var rabbitMQPayload = new
            {
                MessageKey = emailIdempotency.MessageKey,
                EmailId = emailIdempotency.EmailId
            };
            var jsonPayload = JsonConvert.SerializeObject(rabbitMQPayload);
            using (var channel = await this._connection.CreateChannelAsync())
            {
                var props = new BasicProperties
                {
                    Persistent = true
                };
                await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                var body = Encoding.UTF8.GetBytes(jsonPayload);
                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, true, basicProperties: props, body: body);
            }
        }
    }
}
