using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IAiExpenseCategorizationService
    {
        Task<List<ExpenseAiCategory>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<ExpenseAiCategory> categories, CancellationToken cancellationToken = default);
    }
}
