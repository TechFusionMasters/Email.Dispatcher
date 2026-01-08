using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Dto;
using Microsoft.Extensions.Options;

namespace EmailRetryScheduler
{
    public class EmailRetryScheduler : BackgroundService
    {
        private IRabbitMQService _rabbitMQService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RetryPolicyOptions _settings;        

        public EmailRetryScheduler(IOptions<RetryPolicyOptions> settingsAccessor, IRabbitMQService rabbitMQService, IServiceScopeFactory serviceScopeFactory)
        {
            _settings = settingsAccessor.Value;
            _rabbitMQService = rabbitMQService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _rabbitMQService.CreateConnection(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                using (IServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.RescheduleFailedMailsToSend();
                }
                await Task.Delay(_settings.RetryIntervalSeconds * 1000, cancellationToken);
            }
        }
    }
}
