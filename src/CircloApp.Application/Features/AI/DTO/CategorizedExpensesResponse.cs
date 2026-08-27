namespace CircloApp.Application.Features.AI.DTO
{
    public class CategorizedExpensesResponse
    {
        public List<CategorizedExpenseDto> Expenses { get; set; } = new();
    }
}
