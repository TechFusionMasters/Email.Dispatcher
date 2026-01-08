using EmailRetryScheduler.Constant;
using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Data;
using EmailRetryScheduler.Modal;
using Microsoft.EntityFrameworkCore;

namespace EmailRetryScheduler.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private AppDBContext _dBContext;

        public EmailRepository(AppDBContext dBContext) {
            this._dBContext = dBContext;
        }

        public async Task<EmailIdempotency?> GetEmailIdempotencyAsync(Guid emailId) {
            return await _dBContext.EmailIdempotency
                .AsNoTracking()
                .Include(e => e.EmailLog)
                .ThenInclude(e => e.EmailStatus)
                .Where(e => e.EmailId == emailId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> MarkEmailAsDead(Guid emailId) {
            var targetEmailIdempotency = await _dBContext.EmailIdempotency
                .Include(e => e.EmailLog)
                .Where(e => e.EmailId == emailId).FirstOrDefaultAsync();
            if (targetEmailIdempotency == null) return false;
            targetEmailIdempotency.EmailLog.EmailStatusId = (int)Constant.Enum.EmailStatus.Dead;
            targetEmailIdempotency.IsPublished = false;
            await _dBContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkMailForRetry(Guid emailId, DateTime retryAfter) {
            var targetEmailIdempotency = await _dBContext.EmailIdempotency
                .Where(e => e.EmailId == emailId).FirstOrDefaultAsync();
            if (targetEmailIdempotency == null) return false;
            targetEmailIdempotency.EmailLog.NextAttemptAt = retryAfter;
            targetEmailIdempotency.IsPublished = false;
            await _dBContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<EmailIdempotency>> GetRetryMailsForSend() {
            return await _dBContext.EmailIdempotency
                .AsNoTracking()
                .Include(e => e.EmailLog)
                .ThenInclude(e => e.EmailStatus)
                .Where(e => 
                    e.EmailLog.EmailStatusId == (int) Constant.Enum.EmailStatus.Bounced
                    && e.EmailLog.NextAttemptAt < DateTime.Now
                    && e.IsPublished == false
                ).ToListAsync();
        }

        public async Task<bool> MarkMailAsPublished(Guid emailId) {
            var targetEmailIdempotency = await _dBContext.EmailIdempotency
                .Where(e => e.EmailId == emailId).FirstOrDefaultAsync();
            if (targetEmailIdempotency == null) return false;
            targetEmailIdempotency.IsPublished = true;
            targetEmailIdempotency.CompletedAt = null;
            targetEmailIdempotency.EmailLog.SentAt = null;
            targetEmailIdempotency.EmailLog.LockedUntil = null;
            targetEmailIdempotency.EmailLog.NextAttemptAt = null;
            await _dBContext.SaveChangesAsync();
            return true;
        }


        public async Task InsertEmailActionLog(EmailActionLog actionLog) {
            await _dBContext.EmailActionLog.AddAsync(actionLog);
            await _dBContext.SaveChangesAsync();
        }
    }
}
