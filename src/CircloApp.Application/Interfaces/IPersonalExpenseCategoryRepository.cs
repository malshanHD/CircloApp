using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IPersonalExpenseCategoryRepository
    {
        Task<bool> IsAvailableForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default);
        Task<Guid?> GetUncategorizedIdAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PersonalExpenseCategory>> GetAvailableForUserAsync(Guid userId, CancellationToken cancellationTokenq = default);
    }
}
