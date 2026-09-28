using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IPersonalExpenseRepository
    {
        Task AddAsync(PersonalExpense personalExpense, CancellationToken cancellationToken = default);
        Task<PersonalExpense?> GetAsync(Guid userId, Guid id, bool tracking, CancellationToken ct);
        Task<(IReadOnlyList<PersonalExpense> Items, int Count)> ListAsync(Guid userId,
            CircloApp.Application.Features.PersonalExpenses.DTOs.ExpenseFilter filter,
            DateOnly? from, DateOnly? to, CancellationToken ct);
    }
}
