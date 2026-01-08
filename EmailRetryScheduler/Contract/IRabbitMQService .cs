using EmailRetryScheduler.Dto;
using EmailRetryScheduler.Modal;

namespace EmailRetryScheduler.Contract
{
    public interface IRabbitMQService
    {
        Task CreateConnection(CancellationToken cancellationToken);
        Task InsertMessageToRabbitMQ(EmailIdempotency emailIdempotency, string queueName);
    }
}
