namespace CircloApp.Domain.Entities
{
    public class PersonalExpense : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid? CategoryId { get; set; }
        public PersonalExpenseCategory? Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly ExpenseDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Note { get; set; }
    }
}
