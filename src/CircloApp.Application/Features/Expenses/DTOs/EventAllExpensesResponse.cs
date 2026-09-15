using CircloApp.Domain.Enums;

namespace CircloApp.Application.Features.Expenses.DTOs
{
    public class EventAllExpensesResponse
    {
        public string PaidUser { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateAndTime { get; set; }
        public TransactionType Type { get; set; }
    }
}
