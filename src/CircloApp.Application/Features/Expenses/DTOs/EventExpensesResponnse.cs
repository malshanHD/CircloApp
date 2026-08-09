namespace CircloApp.Application.Features.Expenses.DTOs
{
    public class EventExpensesResponnse
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string PaidUser { get; set; } = string.Empty;
        public DateTime DateAndTime { get; set; }
    }
}
