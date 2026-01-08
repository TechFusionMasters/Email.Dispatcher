using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmailRetryScheduler
{
    public class EmailRetryScheduler : BackgroundService
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<EmailRetryScheduler> _logger;

        public EmailRetryScheduler(
            IRabbitMQService rabbitMQService, 
            IServiceScopeFactory serviceScopeFactory,
            ILogger<EmailRetryScheduler> logger)
        {
            _rabbitMQService = rabbitMQService;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _rabbitMQService.CreateConnection(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (IServiceScope scope = _serviceScopeFactory.CreateAsyncScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.RescheduleFailedMailsToSend();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while rescheduling failed emails. The scheduler will continue running.");
                }
                
                await Task.Delay(10000, cancellationToken);
            }
        }
    }
}
