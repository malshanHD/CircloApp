namespace CircloApp.Application.Common.Models
{
    public class PendingRegistration
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string OtpHash { get; set; } = string.Empty;

        public int FailedAttempts { get; set; }
        public DateTime OtpExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
