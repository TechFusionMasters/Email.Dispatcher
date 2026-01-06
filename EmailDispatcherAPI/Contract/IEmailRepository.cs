using EmailDispatcherAPI.Modal;

namespace EmailDispatcherAPI.Contract
{
    public interface IEmailRepository
    {
        Task<EmailIdempotency?> GetEmailIdempotencyAsync(string idempotencyKey);
        Task<EmailLog> CreateEmailLog(EmailLog emailLog);
        Task<EmailIdempotency> CreateEmailIdempotency(EmailIdempotency emailIdempotency);
    }
}
