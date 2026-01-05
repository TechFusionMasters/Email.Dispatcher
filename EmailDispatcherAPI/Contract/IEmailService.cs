namespace EmailDispatcherAPI.Contract
{
    public interface IEmailService
    {
         Task<string> SendEmail();
    }
}
