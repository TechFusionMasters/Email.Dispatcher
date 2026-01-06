using EmailDispatcherAPI.Contract;
using EmailDispatcherAPI.Exception;
using EmailDispatcherAPI.Modal;
using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EmailDispatcherAPI.Service
{
    public class EmailService : IEmailService
    {
        private ConnectionFactory factory;
        private IEmailRepository emailRepository;

        public EmailService(IEmailRepository emailRepository) { 
            this.factory = new ConnectionFactory { HostName = "localhost" };
            this.emailRepository = emailRepository;
        }

        public async Task<bool> IsValidEmail(string email)
        {
            var emailAttr = new EmailAddressAttribute();
            return emailAttr.IsValid(email);
        }


        public async Task SendEmail(string mailTo,int entityId) {
            var emailIdempotency = await this.CreateEmailIdempotency(entityId);
            await this.emailRepository.CreateEmailLog(new Modal.EmailLog
            {
                EmailStatusId = (int)Constant.Enum.EmailStatus.Pending,
                EmailIdempotencyId = emailIdempotency.Id,
                ToAddress = mailTo,
                Subject = "Test Mail",
                Body = "Message through message broker",
                CreatedAt = DateTime.Now,
            });
            await this.InsertMessageToRabbitMQ(emailIdempotency);
        }

        private async Task<EmailIdempotency?> CreateEmailIdempotency(int entityKey) {
            var messageKey = $"SuccessMail:{entityKey}";
            var existsEmailIdempotencyKey = await this.emailRepository.GetEmailIdempotencyAsync(messageKey);
            if (existsEmailIdempotencyKey != null) {
                throw new ResourceAlreadyExistsException("Email already in process");
            }
            return await this.emailRepository.CreateEmailIdempotency(new EmailIdempotency {
                MessageKey = messageKey,
                EmailId = Guid.NewGuid(),
                CreatedAt = DateTime.Now
            });
        }

        private async Task InsertMessageToRabbitMQ(EmailIdempotency emailIdempotency) {
            using (var connection = await factory.CreateConnectionAsync())
            {
                using var channel = await connection.CreateChannelAsync();
                await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false,
                    arguments: null);
                const string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);
                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            }
        }
    }
}