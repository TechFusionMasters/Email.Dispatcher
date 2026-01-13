namespace EmailRetryScheduler.Dto
{
    public class RabitMQDto
    {
        public string MessageKey { get; set; }
        public Guid EmailId { get; set; }
    }
}
