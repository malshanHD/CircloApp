namespace CircloApp.Application.Interfaces.AI
{
    public interface IExpenseRelevanceService
    {
        Task<List<Guid>> GetRelevantExpenseIdsAsync(string question, IReadOnlyList<ExpenseSearchResult> candidates, CancellationToken cancellationToken);
    }
}
