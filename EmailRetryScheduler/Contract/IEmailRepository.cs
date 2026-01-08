using EmailRetryScheduler.Modal;

namespace EmailRetryScheduler.Contract
{
    public interface IEmailRepository
    {
        Task<EmailIdempotency> GetEmailIdempotencyAsync(Guid emailId);
        Task<List<EmailIdempotency>> GetRetryMailsForSend();
        Task<bool> MarkMailAsPublished(Guid id);
        Task InsertEmailActionLog(EmailActionLog actionLog);
    }
}
