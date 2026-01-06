using EmailWorker.Modal;
using Microsoft.EntityFrameworkCore;

namespace EmailWorker.Data
{
    public class AppDBContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public AppDBContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
        }
        public DbSet<EmailLog> EmailLog { get; set; }
        public DbSet<EmailIdempotency> EmailIdempotency { get; set; }
        public DbSet<EmailStatus> EmailStatus { get; set; }
    }
}