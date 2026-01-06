using RabbitMQ.Client;

namespace EmailWorker.Contract
{
    public interface IEmailService
    {
        IConnection Connection { get; }
        Task SendEmail();
    }
}
