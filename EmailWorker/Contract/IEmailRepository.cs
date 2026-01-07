using EmailWorker.Modal;

namespace EmailWorker.Contract
{
    public interface IEmailRepository
    {
        Task<EmailIdempotency> GetEmailIdempotencyAsync(Guid emailId);
        Task<bool> LockEmailSendIdempotency(EmailIdempotency emailIdempotency);
    }
}
