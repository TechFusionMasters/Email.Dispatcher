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
            await this.SendEmailAsync("","Helloe Subject", "Hello Body");
            return;
            var emailIdempotency = await _emailRepository.GetEmailIdempotencyAsync(message.EmailId);
            if (emailIdempotency == null || await this.CheckIsEmialIdempotencyIsSendable(emailIdempotency)) return;
            await _emailRepository.LockEmailSendIdempotency(emailIdempotency);
            
            return;
        }

        private async Task<bool> CheckIsEmialIdempotencyIsSendable(EmailIdempotency em_Id) {
            if ((em_Id.IsPublished && em_Id.CompletedAt == null) && (em_Id.EmailLog.SentAt == null && em_Id.EmailLog.EmailStatusId == (int)Constant.Enum.EmailStatus.Pending && em_Id.EmailLog.LockedUntil < DateTime.Now))
            {
                return true;
            }
            else return false;
        }

        private async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_mailConfig.Name, _mailConfig.FromAddress));
            message.To.Add(new MailboxAddress("", "arulkailasam01@gmail.com"));
            message.Subject = subject;
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"<h1>{subject}</h1><p>{body}</p>",
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
                    Console.WriteLine("Email sent successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}