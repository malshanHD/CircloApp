using CircloApp.Domain.Enums;

namespace CircloApp.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public bool EmailVerified { get; set; } = false;

        public ICollection<BudgetEvent> CreatedEvents { get; set; } = new List<BudgetEvent>();
        public ICollection<EventMember> EventMemberships { get; set; } = new List<EventMember>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<UserExternalLogin> ExternalLogins { get; private set; } = new List<UserExternalLogin>();

        public void AddExternalLogin(string provider, string providerSubject)
        {
            var alreadyLinked = ExternalLogins.Any(x => x.Provider == provider && x.ProviderSubject == providerSubject);
            if (alreadyLinked)
            {
                return;
            }

            ExternalLogins.Add(new UserExternalLogin
            {
                Provider = provider,
                ProviderSubject = providerSubject,
                User = this
            });
        }
    }
}
