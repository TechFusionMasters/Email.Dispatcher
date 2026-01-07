using EmailWorker.Constant;
using EmailWorker.Constant.Enum;
using EmailWorker.Contract;
using EmailWorker.Data;
using EmailWorker.Modal;
using Microsoft.EntityFrameworkCore;

namespace EmailWorker.Repository
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
            await _dBContext.SaveChangesAsync();
            return true;
        }

    }
}
