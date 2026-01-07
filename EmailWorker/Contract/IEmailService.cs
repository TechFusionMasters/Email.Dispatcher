using EmailWorker.Dto;
using RabbitMQ.Client;

namespace EmailWorker.Contract
{
    public interface IEmailService
    {
        Task SendEmail(RabitMQDto message);
    }
}
