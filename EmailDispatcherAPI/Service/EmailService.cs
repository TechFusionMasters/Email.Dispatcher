using EmailDispatcherAPI.Contract;
using RabbitMQ.Client;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace EmailDispatcherAPI.Service
{
    public class EmailService : IEmailService
    {
        private ConnectionFactory factory;

        public EmailService() { 
            this.factory = new ConnectionFactory { HostName = "localhost" };
        }

        public async Task<bool> IsValidEmail(string email)
        {
            var emailAttr = new EmailAddressAttribute();
            return emailAttr.IsValid(email);
        }


        public async Task<string> SendEmail() {
            using (var connection = await factory.CreateConnectionAsync())
            {
                using var channel = await connection.CreateChannelAsync();
                await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false,
                    arguments: null);
                const string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);
                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            }
            return "ok";
        }
    }
}