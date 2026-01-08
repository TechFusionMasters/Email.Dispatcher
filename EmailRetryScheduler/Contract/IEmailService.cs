
using EmailRetryScheduler.Dto;

namespace EmailRetryScheduler.Contract
{
    public interface IEmailService
    {
        Task<bool> MarkMailForRetry(RabitMQDto rabitMQDto);
        Task<bool> RescheduleFailedMailsToSend();
    }
}
