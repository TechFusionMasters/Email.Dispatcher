namespace EmailDispatcherAPI.Contract
{
    public interface IEmailService
    {
        Task<bool> IsValidEmail(string email);
        Task SendEmail(string mailTo,int entityId);
    }
}
