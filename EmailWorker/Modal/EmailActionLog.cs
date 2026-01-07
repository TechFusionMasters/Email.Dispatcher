using System.ComponentModel.DataAnnotations;

namespace EmailWorker.Modal
{
    public class EmailActionLog
    {
        [Key]
        public int Id { get; set; }
        public Guid EmailId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
