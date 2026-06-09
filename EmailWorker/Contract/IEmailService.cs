using EmailWorker.Dto;

namespace EmailWorker.Contract
{
    public interface IEmailService
    {
        Task SendEmail(RabitMQDto message);
    }
}
