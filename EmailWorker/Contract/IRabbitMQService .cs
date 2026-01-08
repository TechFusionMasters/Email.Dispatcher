using EmailWorker.Dto;
using EmailWorker.Modal;

namespace EmailWorker.Contract
{
    public interface IRabbitMQService
    {
        Task CreateConnection(CancellationToken cancellationToken);
        Task InsertMessageToRabbitMQ(EmailIdempotency emailIdempotency, string queueName);
    }
}
