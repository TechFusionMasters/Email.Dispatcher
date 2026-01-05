namespace EmailDispatcherAPI.Contract
{
    public interface IEmailService
    {
        Task<bool> IsValidEmail(string email);
         Task<string> SendEmail();
    }
}
