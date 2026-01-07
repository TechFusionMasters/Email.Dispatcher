using EmailDispatcherAPI.Modal;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EmailDispatcherAPI.Data
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
        public DbSet<EmailActionLog> EmailActionLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmailIdempotency>()
                .HasIndex(e => e.MessageKey)
                .IsUnique();
        }

    }
}