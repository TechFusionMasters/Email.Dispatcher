using EmailWorker.Contract;
using EmailWorker.Modal;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EmailWorker.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;
        private readonly IRabbitMqConnection _rabbitMqConnection;
        private const string _queueName = "EmailDispatcher.Queue";
        private const string _routingKey = "Email.Send";
        public IConnection Connection { get; private set; } = default!;
        
        public EmailService(IEmailRepository emailRepository,IRabbitMqConnection rabbitMqConnection) { 
            _emailRepository = emailRepository;
            _rabbitMqConnection = rabbitMqConnection;
        }

        public async Task SendEmail() {
            return;
        }
    }
}