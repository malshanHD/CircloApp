namespace CircloApp.Domain.Entities
{
    public class ExpenseAiCategory
    {
        public Guid Id { get; set; }

        public Guid ExpenseId { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public Expense Expense { get; set; } = null!;
    }
}
