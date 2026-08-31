namespace CircloApp.Application.Features.AI.DTO
{
    public class ExpenseVectorSearchResult
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Amount { get; set; }
        public double Score { get; set; }
    }
}
