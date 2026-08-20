namespace CircloApp.Application.Features.Expenses.DTOs
{
    public class GetUserAllExpensesResponse
    {
        public decimal TotalExpenses { get; set; }
        public string EventName { get; set; } = string.Empty;
        public Guid EventId { get; set; }
    }
}
