namespace CircloApp.Domain.Entities
{
    public class BudgetEvent : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CreatedByUserId { get; set; }

        public User CreatedByUser { get; set; } = null!;
        public ICollection<EventMember> Members { get; set; } = new List<EventMember>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
