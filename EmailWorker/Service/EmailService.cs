using EmailRetryScheduler.Dto;
using EmailWorker.Constant;
using EmailWorker.Contract;
using EmailWorker.Dto;
using EmailWorker.Modal;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailWorker.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        protected readonly IConfiguration configuration;
        private readonly MailConfig _mailConfig;
        private readonly RetryPolicyOptions _retryPloicyOption;
        private readonly IRabbitMQService _rabbitMQService;

        public EmailService(IEmailRepository emailRepository, IOptions<MailConfig> mailConfig, IOptions<RetryPolicyOptions> retryPolicyOption, IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
            _emailRepository = emailRepository;
            _mailConfig = mailConfig?.Value;
            _retryPloicyOption = retryPolicyOption?.Value;


            if (string.IsNullOrWhiteSpace(_mailConfig.FromAddress) ||
                string.IsNullOrWhiteSpace(_mailConfig.MailDomain) ||
                string.IsNullOrWhiteSpace(_mailConfig.MailPassword) ||
                string.IsNullOrWhiteSpace(_mailConfig.Name))
            {
                throw new InvalidOperationException(
                    "MailConfig is missing required values.");
            }
        }

        public async Task SendEmail(RabitMQDto message) {
            var emailIdempotency = await _emailRepository.GetEmailIdempotencyAsync(message.EmailId);
            if (emailIdempotency == null) return;
            if (await this.ShouldSkipEmailProcessing(emailIdempotency)) return;
            await this.SendEmailAsync(emailIdempotency);
            return;
        }

        private async Task<bool> ShouldSkipEmailProcessing(EmailIdempotency emailIdempotency) {
            var now = DateTime.Now;
            
            if (emailIdempotency.EmailLog.EmailStatusId == (int)Constant.Enum.EmailStatus.Sent)
            {
                return true;
            }
            
            if (emailIdempotency.CompletedAt != null)
            {
                return true;
            }
            
            var isPublished = emailIdempotency.IsPublished;
            var isNotCompleted = emailIdempotency.CompletedAt == null;
            var isPending = emailIdempotency.EmailLog.EmailStatusId == (int) Constant.Enum.EmailStatus.Pending;
            var isNotLocked = emailIdempotency.EmailLog.LockedUntil == null || emailIdempotency.EmailLog.LockedUntil < now;
            var isDueForAttempt = emailIdempotency.EmailLog.NextAttemptAt == null || emailIdempotency.EmailLog.NextAttemptAt <= now;
            var isNotSent = emailIdempotency.EmailLog.SentAt == null;
            var isSuccessFullyLocked = await _emailRepository.LockEmailSendIdempotency(emailIdempotency);

            if (isPublished && isNotCompleted && isPending && isNotLocked && isDueForAttempt && isNotSent && isSuccessFullyLocked)
            {
                return false;
            }
            
            return true;
        }

        private async Task SendEmailAsync(EmailIdempotency emailIdempotency)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_mailConfig.Name, _mailConfig.FromAddress));
            //Empty string may be receiver name
            message.To.Add(new MailboxAddress(string.Empty, emailIdempotency.EmailLog.ToAddress));

            message.Subject = emailIdempotency.EmailLog.Subject;
            var body = emailIdempotency.EmailLog.Body;
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"<h1>{body}</h1>",
                TextBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_mailConfig.MailDomain, 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_mailConfig.FromAddress, _mailConfig.MailPassword);
                    await client.SendAsync(message);
                    DateTime now = DateTime.Now;
                    await _emailRepository.MarkEmailSuccess(emailIdempotency.EmailId, now);
                    await AddActionLog(emailIdempotency.EmailId, "Mail delivered successfully", now);
                }
                catch (Exception ex)
                {
                    if (_retryPloicyOption.MaxAttempts <= emailIdempotency.EmailLog.AttemptCount + 1 ) {
                        try
                        {
                            await _rabbitMQService.InsertMessageToRabbitMQ(emailIdempotency, AppConstant.DLQQueueName);
                            await _emailRepository.MarkEmailAsDead(emailIdempotency.EmailId);
                            await AddActionLog(emailIdempotency.EmailId, $"Dead : Failed mail message inserted to DLQ after reached the maximum attempt", DateTime.Now);
                        }
                        catch (Exception e)
                        {
                            await AddActionLog(emailIdempotency.EmailId, $"Email Worker Failed to insert message to DLQ after reach the maximum attempt. Error Message was : {e.Message}", DateTime.Now);
                        }
                    }
                    else {
                        var retryTime = DateTime.Now.AddMinutes(GetRescheduleMinutes(emailIdempotency.EmailLog.AttemptCount));
                        await _emailRepository.MarkEmailFail(emailIdempotency.EmailId, ex.Message, retryTime);
                        await AddActionLog(emailIdempotency.EmailId, $"Mail delivery failed at attempt of {emailIdempotency.EmailLog.AttemptCount + 1}. And Marked as not published. Error Message was : {ex.Message}", DateTime.Now);
                    }
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }

        private int GetRescheduleMinutes(int attempt)
        {
            var index = attempt - 1;
            if (index < 0) index = 0;
            if (index >= _retryPloicyOption.BackoffScheduleMinutes.Count) index = _retryPloicyOption.BackoffScheduleMinutes.Count - 1;
            return _retryPloicyOption.BackoffScheduleMinutes[index];
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