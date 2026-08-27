namespace CircloApp.Application.Features.AI.DTO
{
    public class CategorizedExpenseDto
    {
        public Guid ExpenseId { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
