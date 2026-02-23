using EmailRetryScheduler.Constant;
using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Modal;

namespace EmailRetryScheduler.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        protected readonly IConfiguration configuration;
        private readonly IRabbitMQService _rabbitMQService;

        public EmailService(IEmailRepository emailRepository, IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
            _emailRepository = emailRepository;
        }

        public async Task<bool> RescheduleFailedMailsToSend() {
            var pendingMailToPublish = await _emailRepository.GetRetryMailsForSend();
            foreach (var item in pendingMailToPublish)
            { 
                try
                {
                   await _rabbitMQService.InsertMessageToRabbitMQ(item, AppConstant.QueueName);
                   await _emailRepository.MarkMailAsPublished(item.EmailId);
                   await AddActionLog(item.EmailId, $"Retry scheduler has successfully added failed mail Message to primary queue for send a retry mail.", DateTime.Now);
                }
                catch (Exception ex)
                {
                    await AddActionLog(item.EmailId, $"Retry scheduler Failed to insert message to primary queue for send a retry mail. Error Message was : {ex.Message}", DateTime.Now);
                }
            }
            return true;
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