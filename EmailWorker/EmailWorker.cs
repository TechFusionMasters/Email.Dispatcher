using EmailWorker.Constant;
using EmailWorker.Contract;
using EmailWorker.Dto;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EmailWorker
{
    public class EmailWorker : BackgroundService
    {
        private readonly ILogger<EmailWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IChannel _channel;

        public EmailWorker(ILogger<EmailWorker> logger,IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            await this.CreateConnection(cancellationToken);
        }

        public async Task CreateConnection(CancellationToken cancellationToken)
        {
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            await this.ListenRabbitMq(cancellationToken);
        }

        private async Task ListenRabbitMq(CancellationToken cancellationToken)
        {
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: AppConstant.QueueName,
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
                if (message == null)
                    return;
               await this.SendEmail(cancellationToken, message);
               await _channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            };

            await _channel.BasicConsumeAsync(
                  queue: AppConstant.QueueName,
                  autoAck: false,
                  consumer: consumer);
        }

        private async Task SendEmail(CancellationToken stoppingToken, RabitMQDto message)
        {
            if (stoppingToken.IsCancellationRequested) return;
            using (IServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
            {
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendEmail(message);
            }
        }

    }
}
