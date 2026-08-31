using CircloApp.Application.Features.AI.DTO;

namespace CircloApp.Application.Interfaces
{
    public interface IExpenseVectorSearchService
    {
        Task CreateIndexAsync(CancellationToken cancellationToken = default);
        Task UploadExpenseAsync(Guid expenseId, Guid eventId, string description, decimal amount, CancellationToken cancellationToken = default);
        Task DeleteIndexAsync(CancellationToken cancellationToken = default);
        Task<List<ExpenseVectorSearchResult>> SearchExpenseAsync(Guid eventId, string query, CancellationToken cancellationToken = default);
    }
}
