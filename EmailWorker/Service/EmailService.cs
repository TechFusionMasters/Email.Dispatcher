using EmailWorker.Contract;
using EmailWorker.Dto;
using EmailWorker.Modal;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;

namespace EmailWorker.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        protected readonly IConfiguration configuration;
        private readonly MailConfig _mailConfig;

        public EmailService(IEmailRepository emailRepository, IOptions<MailConfig> mailConfig)
        {
            _emailRepository = emailRepository;
            _mailConfig = mailConfig?.Value;

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
            if (emailIdempotency == null || await this.CheckIsEmialIdempotencyIsSendable(emailIdempotency)) return;
            await _emailRepository.LockEmailSendIdempotency(emailIdempotency);
            await this.SendEmailAsync(emailIdempotency);
            return;
        }

        private async Task<bool> CheckIsEmialIdempotencyIsSendable(EmailIdempotency em_Id) {
            if ((em_Id.IsPublished && em_Id.CompletedAt == null) && (em_Id.EmailLog.SentAt == null && em_Id.EmailLog.EmailStatusId == (int)Constant.Enum.EmailStatus.Pending && em_Id.EmailLog.LockedUntil < DateTime.Now))
            {
                return true;
            }
            else return false;
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
                    await _emailRepository.MarkEmailFail(emailIdempotency.EmailId, ex.Message);
                    await AddActionLog(emailIdempotency.EmailId, $"Mail delivery failed at attempt of {emailIdempotency.EmailLog.AttemptCount}. Error Message was : {ex.Message}", DateTime.Now);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
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