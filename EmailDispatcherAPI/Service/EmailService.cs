using EmailDispatcherAPI.Constant;
using EmailDispatcherAPI.Contract;
using EmailDispatcherAPI.Exception;
using EmailDispatcherAPI.Modal;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EmailDispatcherAPI.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        private readonly IRabbitMqConnection _rabbitMqConnection;

        public EmailService(IEmailRepository emailRepository, IRabbitMqConnection rabbitMqConnection) { 
            this._emailRepository = emailRepository;
            this._rabbitMqConnection = rabbitMqConnection;
        }

        public async Task<bool> IsValidEmail(string email)
        {
            var emailAttr = new EmailAddressAttribute();
            return emailAttr.IsValid(email);
        }


        public async Task SendEmail(string mailTo,int entityId) {
            var emailIdempotency = await this.CreateEmailIdempotency(entityId);
            await this._emailRepository.CreateEmailLog(new Modal.EmailLog
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
            var existsEmailIdempotencyKey = await this._emailRepository.GetEmailIdempotencyAsync(messageKey);
            if (existsEmailIdempotencyKey != null) {
                throw new ResourceAlreadyExistsException("Email already in process");
            }
            return await this._emailRepository.CreateEmailIdempotency(new EmailIdempotency
            {
                MessageKey = messageKey,
                EmailId = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                IsPublished = false
            });
        }

        private async Task InsertMessageToRabbitMQ(EmailIdempotency emailIdempotency) {
            var rabbitMQPayload = new { 
                MessageKey = emailIdempotency.MessageKey,
                EmailId = emailIdempotency.EmailId
            };
            var jsonPayload = JsonConvert.SerializeObject(rabbitMQPayload);
            using (var channel = await this._rabbitMqConnection.Connection.CreateChannelAsync())
            {
                var props = new BasicProperties
                {
                    Persistent = true
                };
                await channel.QueueDeclareAsync(queue: AppConstant.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                var body = Encoding.UTF8.GetBytes(jsonPayload);
                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: AppConstant.QueueName, true, basicProperties: props, body: body);
            }
            await this._emailRepository.MarkEmailIdempotencyAsPublishedAsync(emailIdempotency.Id);
        }
    }
}