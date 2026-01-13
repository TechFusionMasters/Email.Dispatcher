using EmailRetryScheduler.Dto;
using EmailWorker.Contract;
using EmailWorker.Data;
using EmailWorker.Dto;
using EmailWorker.Repository;
using EmailWorker.Service;
using Microsoft.Extensions.Configuration;

namespace EmailWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddDbContext<AppDBContext>();
            builder.Services.AddScoped<IEmailRepository, EmailRepository>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));
            builder.Services.Configure<RetryPolicyOptions>(builder.Configuration.GetSection("RetryPolicy"));
            builder.Services.Configure<MailConfig>(builder.Configuration.GetSection("MailConfig"));
            builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
            builder.Services.AddHostedService<EmailWorker>();


            var host = builder.Build();
            host.Run();
        }
    }
}