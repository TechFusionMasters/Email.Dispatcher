namespace EmailWorker.Constant
{
    internal class AppConstant
    {
        public const string QueueName = "email.dispatcher.queue";
        public const int LeaseLockTime = 10;
    }
}
