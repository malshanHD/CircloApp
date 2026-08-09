namespace CircloApp.Application.Features.Expenses.DTOs
{
    public class CreateExpensesRequest
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
