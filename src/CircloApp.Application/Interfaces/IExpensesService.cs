using CircloApp.Application.Features.AI.DTO;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IExpensesService
    {
        Task AddExpense(Expense expense, CancellationToken cancellationToken);
        Task AddRangeAsync(IEnumerable<Expense> expenses, CancellationToken cancellationToken);
        Task<List<EventExpensesResponnse>> GetEventExpenses(Guid eventId, CancellationToken cancellationToken);
        Task<List<GetUserAllExpensesResponse>> GetUserExpensesByEventAsync(Guid userId, CancellationToken cancellationToken);
        Task<List<GetUserMonthlyExpensesResponse>> GetUserExpensesByMonth(Guid userId,int year, CancellationToken cancellationToken);
        Task<List<MemberSpendingDto>> GetMemberSpendings(Guid eventId, CancellationToken cancellationToken = default);
    }
}
