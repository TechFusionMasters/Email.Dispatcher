using EmailRetryScheduler.Contract;
using EmailRetryScheduler.Data;
using EmailRetryScheduler.Dto;
using EmailRetryScheduler.Repository;
using EmailRetryScheduler.Service;

namespace EmailRetryScheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddDbContext<AppDBContext>();
            builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));
            builder.Services.AddScoped<IEmailRepository, EmailRepository>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
            builder.Services.AddHostedService<EmailRetryScheduler>();

            var host = builder.Build();
            host.Run();
        }
    }
}