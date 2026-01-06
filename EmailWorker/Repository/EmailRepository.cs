using EmailWorker.Contract;
using EmailWorker.Data;
using EmailWorker.Modal;
using Microsoft.EntityFrameworkCore;

namespace EmailWorker.Repository
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
    }
}
