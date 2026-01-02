using System.ComponentModel.DataAnnotations;

namespace EmailDispatcherAPI.Modal
{
    public class EmailLog
    {
        [Key]
        public int Id { get; set; }
        public int EmailStatusId { get; set; }
        public EmailStatus? EmailStatus { get; set; }
    }
}
