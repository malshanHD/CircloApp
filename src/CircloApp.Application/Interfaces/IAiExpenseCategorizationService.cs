namespace CircloApp.Application.Interfaces
{
    public interface IAiExpenseCategorizationService
    {
        Task<string> AnalyzeSpendingAsync(string expensesDataJson, CancellationToken cancellationToken = default);
    }
}
