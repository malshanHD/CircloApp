using CircloApp.Domain.Enums;

namespace CircloApp.Application.Features.Expenses.DTOs
{
    public class EventExpensesResponnse
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string PaidUser { get; set; } = string.Empty;
        public Guid PaidUserId { get; set; }
        public Guid PaidToUserId{ get; set; }
        public DateTime DateAndTime { get; set; }
        public TransactionType Type { get; set; }
    }
}
