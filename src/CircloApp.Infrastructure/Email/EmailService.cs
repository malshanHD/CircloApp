using CircloApp.Application.Interfaces;
using MailKit.Security;
using MimeKit;

namespace CircloApp.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService(Microsoft.Extensions.Options.IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailNotification(string email, string subject, string htmlBody)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.Email));

            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body = builder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);

            await client.SendAsync(message);
        }

        public async Task SendOtpAsync(string email, string name, string otp)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.Email));

            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = "Verify your Circlo account";

            var builder = new BodyBuilder();

            builder.HtmlBody = $@"
                    <html>
                    <body style='font-family:Arial'>

                    <h2>Welcome to Circlo 👋</h2>

                    <p>Hello <b>{name}</b>,</p>

                    <p>Your verification code is</p>

                    <h1 style='letter-spacing:5px;color:#2563eb'>
                    {otp}
                    </h1>

                    <p>
                    This OTP expires in
                    <b>5 minutes</b>.
                    </p>

                    <p>
                    If you didn't request this,
                    please ignore this email.
                    </p>

                    <hr>

                    <p>
                    Circlo Team
                    </p>

                    </body>
                    </html>";

            message.Body = builder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }
    }
}
