using CircloApp.Domain.Enums;

namespace CircloApp.Application.Features.Expenses.DTOs
{
    public class CreateExpensesRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}
