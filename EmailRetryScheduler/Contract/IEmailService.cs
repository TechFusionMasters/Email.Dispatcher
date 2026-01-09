
namespace EmailRetryScheduler.Contract
{
    public interface IEmailService
    {
        Task<bool> RescheduleFailedMailsToSend();
    }
}
