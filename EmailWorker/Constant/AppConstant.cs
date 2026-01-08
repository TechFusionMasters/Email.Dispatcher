namespace EmailWorker.Constant
{
    internal class AppConstant
    {
        public const string QueueName = "email.dispatcher.send";
        public const string DLQQueueName = "email.dispatcher.dlq";
        public const int LeaseLockTime = 10;
    }
}
