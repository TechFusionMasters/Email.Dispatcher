using EmailWorker.Modal;

namespace EmailWorker.Contract
{
    public interface IEmailRepository
    {
        Task<EmailIdempotency> GetEmailIdempotencyAsync(Guid emailId);
        Task<bool> LockEmailSendIdempotency(EmailIdempotency emailIdempotency);
        Task<bool> MarkEmailSuccess(Guid emailId, DateTime actionAt);
        Task<bool> MarkEmailFail(Guid emailId, string lastError, DateTime retryTime);
        Task InsertEmailActionLog(EmailActionLog actionLog);
        Task<bool> MarkEmailAsDead(Guid emailId);
    }
 }
