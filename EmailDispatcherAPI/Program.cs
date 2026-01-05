
using EmailDispatcherAPI.Contract;
using EmailDispatcherAPI.Data;
using EmailDispatcherAPI.Service;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace EmailDispatcherAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<AppDBContext>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddEndpointsApiExplorer(); // Required for minimal APIs to discover endpoints
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
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            var summaries = new[]
            {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

            app.MapGet("/sendEmail", async (HttpContext httpContext, IEmailService emailService, string mailTo) =>
            {
                if (mailTo.IsNullOrEmpty() || !await emailService.IsValidEmail(mailTo)) {
                    return Results.BadRequest("Invalid Email");
                }
                await emailService.SendEmail();
                return Results.Ok();
            })
            .WithName("SendEmailNotification");

            app.Run();
        }
    }
}