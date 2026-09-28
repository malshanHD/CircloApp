namespace CircloApp.Application.Features.PersonalExpenses.DTOs
{
    public class PersonalExpensesResponse
    {
        public Guid Id { get; internal set; }
        public string Description { get; internal set; } = string.Empty;
        public decimal Amount { get; internal set; }
        public DateOnly ExpenseDate { get; internal set; }
        public Guid? CategoryId { get; internal set; }
        public string? PaymentMethod { get; internal set; }
        public string? Note { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
    }
}
