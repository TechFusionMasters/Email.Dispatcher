
namespace EmailWorker.Dto
{
    public class MailConfig
    {
        public string FromAddress { get; set; } = null!;
        public string Name { get; set; } = null!;

        public string MailDomain { get; set; } = null!;
        public string MailPassword { get; set; } = null!;
        public int FirstRetryAttemptTimeSpanInMinutes { get; set; } = 1;
    }

}
