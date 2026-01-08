namespace EmailWorker.Constant.Enum
{
    public enum EmailStatus
    {
        Pending = 1,
        Scheduled = 2,
        Sent = 3,
        Failed = 4,
        RetryQueued = 5,
        Dead = 6
    }
}
