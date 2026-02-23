using EmailDispatcherAPI.Contract;
using EmailDispatcherAPI.Data;
using EmailDispatcherAPI.Modal;
using Microsoft.EntityFrameworkCore;

namespace EmailDispatcherAPI.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private AppDBContext dBContext;

        public EmailRepository(AppDBContext dBContext) {
            this.dBContext = dBContext;
        }

        public async Task<EmailIdempotency?> GetEmailIdempotencyAsync(string idempotencyKey) {
            return await this.dBContext.EmailIdempotency.AsNoTracking().FirstOrDefaultAsync(e => e.MessageKey == idempotencyKey);
        }

        public async Task<EmailLog> CreateEmailLog(EmailLog emailLog) {
            await this.dBContext.EmailLog.AddAsync(emailLog);
            await this.dBContext.SaveChangesAsync();
            return emailLog;
        }

        public async Task<EmailIdempotency> CreateEmailIdempotency(EmailIdempotency emailIdempotency) {
            await this.dBContext.EmailIdempotency.AddAsync(emailIdempotency);
            await this.dBContext.SaveChangesAsync();
            return emailIdempotency;
        }

        public async Task<bool> MarkEmailIdempotencyAsPublishedAsync(int id) {
            var emailIdempotency = await this.dBContext.EmailIdempotency.FindAsync(id);
            emailIdempotency.IsPublished = true;
            return await this.dBContext.SaveChangesAsync() != default(int) ? true : false;
        }

    }
}
