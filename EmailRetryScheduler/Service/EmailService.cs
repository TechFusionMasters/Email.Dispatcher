using EmailRetryScheduler.Constant;
using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Dto;
using EmailRetryScheduler.Modal;
using Microsoft.Extensions.Options;

namespace EmailRetryScheduler.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        protected readonly IConfiguration configuration;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly RetryPolicyOptions _settings;

        public EmailService(IOptions<RetryPolicyOptions> settingsAccessor, IEmailRepository emailRepository, IRabbitMQService rabbitMQService)
        {
            _settings = settingsAccessor.Value;
            _rabbitMQService = rabbitMQService;
            _emailRepository = emailRepository;
        }

        public async Task<bool> MarkMailForRetry(RabitMQDto message) {
            var emailIdempotency = await _emailRepository.GetEmailIdempotencyAsync(message.EmailId);
            if (emailIdempotency == null) return false;
            if (emailIdempotency.EmailLog.AttemptCount >= _settings.MaxAttempts) { 
                await _emailRepository.MarkEmailAsDead(emailIdempotency.EmailId);
                await AddActionLog(emailIdempotency.EmailId, "Due To Maximum Attempt reach this mail is marked as Dead", DateTime.Now);
                return true;
            }
            var rescheduleMinutes = GetRescheduleMinutes(emailIdempotency.EmailLog.AttemptCount);
            var now = DateTime.Now.AddMinutes(rescheduleMinutes);
            await _emailRepository.MarkMailForRetry(emailIdempotency.EmailId, now);
            await AddActionLog(emailIdempotency.EmailId, $"Mail marked for retry and will run at {now:O}", DateTime.Now);
            return true;
        }

        public async Task<bool> RescheduleFailedMailsToSend() {
            var pendingMailToPublish = await _emailRepository.GetRetryMailsForSend();
            foreach (var item in pendingMailToPublish)
            { 
                try
                {
                   await _rabbitMQService.InsertMessageToRabbitMQ(item, AppConstant.QueueName);
                   await _emailRepository.MarkMailAsPublished(item.EmailId);
                   await AddActionLog(item.EmailId, $"By Retry scheduler successfully failed mail Message inserted to primary queue for send a retry mail.", DateTime.Now);
                }
                catch (Exception ex)
                {
                    await AddActionLog(item.EmailId, $"Retry scheduler Failed to insert message to primary queue for send a retry mail. Error Message was : {ex.Message}", DateTime.Now);
                }
            }
            return true;
        }

        private int GetRescheduleMinutes(int attempt)
        {
            var index = attempt - 1;
            if (index < 0) index = 0;
            if (index >= _settings.BackoffScheduleMinutes.Count) index = _settings.BackoffScheduleMinutes.Count - 1;
            return _settings.BackoffScheduleMinutes[index];
        }

        private async Task<bool> AddActionLog(Guid emailId, string message ,DateTime? actionAt) {
            var emailAction = new EmailActionLog
            {
                EmailId = emailId,
                Message = message,
                CreatedAt = actionAt ?? DateTime.Now
            };
            await _emailRepository.InsertEmailActionLog(emailAction);
            return true;
        }
    }
}