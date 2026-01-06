
using EmailDispatcherAPI.Contract;
using EmailDispatcherAPI.Data;
using EmailDispatcherAPI.Exception;
using EmailDispatcherAPI.Repository;
using EmailDispatcherAPI.Service;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace EmailDispatcherAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddDbContext<AppDBContext>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IEmailRepository, EmailRepository>();

            //Asynchronous Initialization via Hosted Service
            builder.Services.AddSingleton<RabbitMqConnection>();
            builder.Services.AddSingleton<IRabbitMqConnection>(
                sp => sp.GetRequiredService<RabbitMqConnection>()
            );

            builder.Services.AddHostedService<RabbitMqHostedService>();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My Minimal API", Version = "v1" });
            });
            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler();
            }
            app.UseHttpsRedirection();

            app.UseAuthorization();

            var summaries = new[]
            {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

            app.MapPost("/sendEmail", async (HttpContext httpContext, IEmailService emailService,string mailTo,int entityId) =>
            {
                if (mailTo.IsNullOrEmpty() || !await emailService.IsValidEmail(mailTo)) {
                    throw new ArgumentException("Invalid Email ID");
                }
                if (entityId == default(int) || entityId < 1) {
                    throw new ArgumentException("Invalid Entity Id");
                }
                await emailService.SendEmail(mailTo, entityId);
                return Results.Ok("Mail Scheduled SuccessFully");
            })
            .WithName("SendEmailNotification");

            app.Run();
        }
    }
}