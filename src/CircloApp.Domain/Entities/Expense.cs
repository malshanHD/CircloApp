using CircloApp.Domain.Enums;

namespace CircloApp.Domain.Entities
{
    public class Expense : BaseEntity
    {
        public Guid EventId { get; set; }
        public Guid PaidByUserId { get; set; }
        public Guid PaidToUserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ExpenseDate { get; set; }
        public TransactionType Type { get; set; } 

        public BudgetEvent Event { get; set; } = null!;
        public User PaidByUser { get; set; } = null!;
    }
}
