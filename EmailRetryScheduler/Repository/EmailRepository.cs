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

        public async Task<bool> LockEmailSendIdempotency(EmailIdempotency emailIdempotency) {
            var targetEmailIdempotency = await _dBContext.EmailIdempotency
                .Include(e => e.EmailLog)
                .Where(e => e.Id == emailIdempotency.Id).FirstOrDefaultAsync();
            if (targetEmailIdempotency == null) return false;
            targetEmailIdempotency.EmailLog.EmailStatusId = (int)Constant.Enum.EmailStatus.Scheduled;
            targetEmailIdempotency.EmailLog.LockedUntil = DateTime.Now.AddSeconds(AppConstant.LeaseLockTime);
            targetEmailIdempotency.EmailLog.AttemptCount += targetEmailIdempotency.EmailLog.AttemptCount;
            await _dBContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkEmailSuccess(Guid emailId, DateTime actionAt) {
            var targetEmailIdempotency = await _dBContext.EmailIdempotency
                .Include(e => e.EmailLog)
                .Where(e => e.EmailId == emailId).FirstOrDefaultAsync();
            if (targetEmailIdempotency == null) return false;
            targetEmailIdempotency.EmailLog.EmailStatusId = (int)Constant.Enum.EmailStatus.Sent;
            targetEmailIdempotency.EmailLog.SentAt = actionAt;
            targetEmailIdempotency.EmailLog.LockedUntil = null;
            targetEmailIdempotency.EmailLog.LastError = null;
            targetEmailIdempotency.CompletedAt = actionAt;
            await _dBContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkEmailFail(Guid emailId, string lastError)
        {
            var targetEmailIdempotency = await _dBContext.EmailIdempotency
                .Include(e => e.EmailLog)
                .Where(e => e.EmailId == emailId).FirstOrDefaultAsync();
            if (targetEmailIdempotency == null) return false;
            targetEmailIdempotency.EmailLog.EmailStatusId = (int)Constant.Enum.EmailStatus.Bounced;
            targetEmailIdempotency.EmailLog.LockedUntil = null;
            targetEmailIdempotency.EmailLog.LastError = lastError;
            await _dBContext.SaveChangesAsync();
            return true;
        }

        public async Task InsertEmailActionLog(EmailActionLog actionLog) {
            await _dBContext.EmailActionLog.AddAsync(actionLog);
            await _dBContext.SaveChangesAsync();
        }
    }
}
