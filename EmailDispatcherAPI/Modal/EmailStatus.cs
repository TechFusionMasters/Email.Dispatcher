using System.ComponentModel.DataAnnotations;

namespace EmailDispatcherAPI.Modal
{
    public class EmailStatus
    {
        [Key]
        public int Id { get; set; }
        public string Status { get; set; }
    }
}
