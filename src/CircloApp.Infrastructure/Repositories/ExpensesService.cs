using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.Infrastructure.Repositories
{
    public class ExpensesService : IExpensesService
    {
        private readonly ApplicationDbContext _context;
        public ExpensesService(ApplicationDbContext applicationDb)
        {
            _context = applicationDb;
        }
        public async Task AddExpense(Expense expense, CancellationToken cancellationToken)
        {
            await _context.Expenses.AddAsync(expense);
        }

        public async Task AddRangeAsync(IEnumerable<Expense> expenses, CancellationToken cancellationToken)
        {
            await _context.Expenses.AddRangeAsync(expenses, cancellationToken);
        }

        public async Task<List<EventExpensesResponnse>> GetEventExpenses(Guid eventId, CancellationToken cancellationToken)
        {
            return await _context.Expenses
                .AsNoTracking()
                .Where(x => x.EventId == eventId)
                .Select(e => new EventExpensesResponnse
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Description = e.Description,
                    DateAndTime = e.CreatedAt,
                    PaidUser = e.PaidByUser.FirstName,
                    PaidUserId = e.PaidByUserId,
                    PaidToUserId = e.PaidToUserId,
                    Type = e.Type
                }).ToListAsync(cancellationToken);
        }
    }
}
