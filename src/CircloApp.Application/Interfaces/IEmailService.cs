namespace CircloApp.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpAsync(string email, string name, string otp);
        Task SendEmailNotification(string email, string subject, string htmlBody);
    }
}
