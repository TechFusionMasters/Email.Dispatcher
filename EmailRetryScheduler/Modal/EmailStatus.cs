using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmailRetryScheduler.Modal
{
    public class EmailStatus
    {
        [Key]
        public int Id { get; set; }
        public string Status { get; set; }
    }
}
