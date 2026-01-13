using EmailWorker.Contract;

namespace EmailWorker
{
    public class EmailWorker : BackgroundService
    {
        private IRabbitMQService _rabbitMQService;

        public EmailWorker(IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            await _rabbitMQService.CreateConnection(cancellationToken);
        }
    }
}
