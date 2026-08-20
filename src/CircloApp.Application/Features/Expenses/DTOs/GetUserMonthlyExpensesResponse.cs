namespace CircloApp.Application.Features.Expenses.DTOs
{
    public class GetUserMonthlyExpensesResponse
    {
        public decimal TotalAmount { get; set; }
        public string Month { get; set; } = string.Empty;
    }
}
