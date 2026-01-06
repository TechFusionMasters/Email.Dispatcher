using EmailWorker.Contract;
using EmailWorker.Data;
using EmailWorker.Repository;
using EmailWorker.Service;

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
            //Asynchronous Initialization via Hosted Service
            builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
            builder.Services.AddHostedService<RabbitMqHostedService>();
            builder.Services.AddHostedService<EmailWorker>();

            var host = builder.Build();
            host.Run();
        }
    }
}