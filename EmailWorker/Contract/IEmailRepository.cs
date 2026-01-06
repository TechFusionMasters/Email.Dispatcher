using EmailWorker.Modal;

namespace EmailWorker.Contract
{
    public interface IEmailRepository
    {
        Task<EmailIdempotency?> GetEmailIdempotencyAsync(string idempotencyKey);
    }
}
