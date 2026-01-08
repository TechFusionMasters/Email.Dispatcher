using EmailRetryScheduler.Constant;
using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Dto;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EmailRetryScheduler
{
    public class EmailRetryScheduler : BackgroundService
    {
        private IRabbitMQService _rabbitMQService;

        public EmailRetryScheduler(IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _rabbitMQService.CreateConnection(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                await _rabbitMQService.CheckAndRescheduleMail();
                await Task.Delay(10000, cancellationToken);
            }
        }
    }
}
