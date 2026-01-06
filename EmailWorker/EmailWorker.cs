using EmailWorker.Contract;
using RabbitMQ.Client;

namespace EmailWorker
{
    public class EmailWorker : BackgroundService
    {
        private readonly ILogger<EmailWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;

        public EmailWorker(ILogger<EmailWorker> logger,IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {            
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                using (IServiceScope scope = _serviceScopeFactory.CreateAsyncScope()) {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendEmail();
                }
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
