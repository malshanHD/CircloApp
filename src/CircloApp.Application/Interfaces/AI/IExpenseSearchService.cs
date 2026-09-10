namespace CircloApp.Application.Interfaces.AI
{
    public interface IExpenseSearchService
    {
        Task<IReadOnlyList<ExpenseSearchResult>> SearchExpensesAsync(Guid eventId, string query, CancellationToken cancellationToken);
    }

    public record ExpenseSearchResult(Guid ExpenseId, string Description, decimal Amount, double Score);
}
