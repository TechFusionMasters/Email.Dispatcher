using System.ComponentModel.DataAnnotations;

namespace EmailWorker.Modal
{
    public class EmailIdempotency
    {
        [Key]
        public int Id { get; set; }
        public string MessageKey { get; set; }
        public Guid EmailId { get; set; } 
        public bool IsPublished { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
