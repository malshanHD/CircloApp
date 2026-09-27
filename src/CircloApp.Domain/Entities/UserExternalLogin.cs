namespace CircloApp.Domain.Entities
{
    public class UserExternalLogin
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string ProviderSubject { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public ICollection<UserExternalLogin> ExternalLogins { get; set; } = new List<UserExternalLogin>();
    }
}
