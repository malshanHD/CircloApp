namespace CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis
{
    public class ExpenseAnalysisResponse
    {
        public decimal TotalExpense { get; set; }
        public List<CategorySummaryDto> Categories { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    public class CategorySummaryDto
    {
        public string Category { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal Percentage { get; set; }
    }
}
