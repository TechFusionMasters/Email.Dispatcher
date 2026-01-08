using EmailRetryScheduler.Constant;
using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Dto;
using EmailRetryScheduler.Modal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EmailRetryScheduler
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ILogger<EmailRetryScheduler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        public async Task CreateConnection(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            await this.ListenRabbitMq(cancellationToken);
        }

        private async Task ListenRabbitMq(CancellationToken cancellationToken)
        {
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: AppConstant.DLQQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                if (cancellationToken.IsCancellationRequested) return;
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert.DeserializeObject<RabitMQDto>(messageJson);
                if (message == null || (message?.MessageKey.IsNullOrEmpty() ?? true) || (Guid.TryParse(message?.EmailId.ToString(),out _)))
                    return;
                await MarkMailForRetry(cancellationToken, message);
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            };

            await _channel.BasicConsumeAsync(
                  queue: AppConstant.QueueName,
                  autoAck: false,
                  consumer: consumer);
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

        private async Task MarkMailForRetry(CancellationToken stoppingToken, RabitMQDto message)
        {
            if (stoppingToken.IsCancellationRequested) return;
            using (IServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.MarkMailForRetry(message);
            }
        }

    }
}
