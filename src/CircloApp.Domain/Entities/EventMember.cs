namespace CircloApp.Domain.Entities
{
    public class EventMember : BaseEntity
    {
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = "Member"; // Default role is "Member"
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
        public BudgetEvent Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
