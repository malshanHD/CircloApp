namespace CircloApp.Infrastructure.Email
{
    public class EmailSettings
    {
        public const string SectionName = "EmailSettings";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
