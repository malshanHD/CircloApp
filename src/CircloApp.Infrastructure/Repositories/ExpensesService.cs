using CircloApp.Application.Features.AI.DTO;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Domain.Enums;
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

        public async Task<List<MemberSpendingDto>> GetMemberSpendings(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.Expenses.AsNoTracking()
                                          .Where(x => x.EventId == eventId)
                                          .GroupBy(x => new
                                          {
                                              x.PaidToUserId,
                                              x.PaidByUser.FirstName,
                                              x.PaidByUser.LastName
                                          })
                                          .Select(group => new MemberSpendingDto
                                          {
                                              UserId = group.Key.PaidToUserId,
                                              Name = group.Key.FirstName + " " + group.Key.LastName,
                                              TotalPaid = group.Sum(x => x.Amount)
                                          })
                                          .OrderByDescending(x => x.TotalPaid)
                                          .ToListAsync(cancellationToken);
        }

        public async Task<List<GetUserAllExpensesResponse>> GetUserExpensesByEventAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Expenses
             .Where(e => e.PaidByUserId == userId || e.PaidToUserId == userId)
             .GroupBy(e => new { e.EventId, e.Event.Name })
             .Select(g => new GetUserAllExpensesResponse
             {
                 EventId = g.Key.EventId,
                 EventName = g.Key.Name,
                 TotalExpenses = g.Sum(e =>
                                e.Type == TransactionType.Expense ? e.Amount :
                                e.Type == TransactionType.Settlement ? -e.Amount : 0m)
             })
             .ToListAsync(cancellationToken);
        }

        public async Task<List<GetUserMonthlyExpensesResponse>> GetUserExpensesByMonth(Guid userId, int year, CancellationToken cancellationToken)
        {
            var response =  await _context.Expenses
                .Where(e => e.PaidByUserId == userId || e.PaidToUserId == userId && e.CreatedAt.Year == year)
                .GroupBy(e => e.CreatedAt.Month)
                .Select(g => new 
                {
                    MonthNumber = g.Key,
                    TotalAmount = g.Sum(e =>
                                e.Type == TransactionType.Expense ? e.Amount :
                                e.Type == TransactionType.Settlement ? -e.Amount : 0m)
                })
                .OrderBy(g => g.MonthNumber)
                .ToListAsync(cancellationToken);

            return response.Select(x => new GetUserMonthlyExpensesResponse
            {
                Month = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(x.MonthNumber),
                TotalAmount = x.TotalAmount
            }).ToList();
        }
    }
}
