using EmailRetryScheduler.Dto;
using RabbitMQ.Client;

namespace EmailRetryScheduler.Contract
{
    public interface IEmailService
    {
        Task SendEmail(RabitMQDto message);
    }
}
