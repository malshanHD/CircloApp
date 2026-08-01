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

        public ICollection<BudgetEvent> CreatedEvents { get; set; } = new List<BudgetEvent>();
        public ICollection<EventMember> EventMemberships { get; set; } = new List<EventMember>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
