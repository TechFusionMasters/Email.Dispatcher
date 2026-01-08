using EmailRetryScheduler.Modal;

namespace EmailRetryScheduler.Contract
{
    public interface IEmailRepository
    {
        Task<EmailIdempotency> GetEmailIdempotencyAsync(Guid emailId);
        Task<bool> LockEmailSendIdempotency(EmailIdempotency emailIdempotency);
        Task<bool> MarkEmailSuccess(Guid emailId,DateTime actionAt);
        Task<bool> MarkEmailFail(Guid emailId, string lastError);
        Task InsertEmailActionLog(EmailActionLog actionLog);

        }
    }
