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
