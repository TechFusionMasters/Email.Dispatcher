using System.ComponentModel.DataAnnotations;

namespace EmailDispatcherAPI.Modal
{
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }
        public int AttemptCount { get; set; }
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? LockedUntil { get; set; }
        public string? LastError { get; set; }
        public int EmailStatusId { get; set; }
        public EmailStatus? EmailStatus { get; set; }
        public int EmailIdempotencyId { get; set; }
        public EmailIdempotency? EmailIdempotency { get; set; }

    }
}
