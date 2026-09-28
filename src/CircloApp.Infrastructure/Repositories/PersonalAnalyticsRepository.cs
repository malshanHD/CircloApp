using CircloApp.Application.Features.PersonalExpenses.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CircloApp.Infrastructure.Repositories;
public class PersonalAnalyticsRepository(ApplicationDbContext context) : IPersonalAnalyticsRepository
{
    private IQueryable<PersonalExpense> Range(Guid user, DateOnly from, DateOnly to) => context.PersonalExpenses
        .AsNoTracking().Where(e => e.UserId == user && !e.IsDeleted && e.ExpenseDate >= from && e.ExpenseDate < to);
    public async Task<decimal> TotalAsync(Guid user, DateOnly from, DateOnly toExclusive, CancellationToken ct) =>
        await Range(user, from, toExclusive).SumAsync(e => (decimal?)e.Amount, ct) ?? 0;
    public async Task<ExpenseStatistics> StatisticsAsync(Guid user, DateOnly from, DateOnly toExclusive, CancellationToken ct)
    {
        var query = Range(user, from, toExclusive);
        var total = await query.SumAsync(e => (decimal?)e.Amount, ct) ?? 0;
        var count = await query.CountAsync(ct);
        var highest = await query.OrderByDescending(e => e.Amount).ThenByDescending(e => e.ExpenseDate).ThenBy(e => e.Id).Include(e => e.Category).FirstOrDefaultAsync(ct);
        var recent = await query.OrderByDescending(e => e.ExpenseDate).ThenByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id)
            .Include(e => e.Category).Take(5).ToListAsync(ct);
        var categories = await query.GroupBy(e => new { e.CategoryId, Name = e.Category == null ? "Uncategorized" : e.Category.Name, Color = e.Category == null ? null : e.Category.Color })
            .Select(g => new { g.Key.CategoryId, g.Key.Name, g.Key.Color, Total = g.Sum(e => e.Amount), Count = g.Count() })
            .OrderByDescending(g => g.Total).ThenBy(g => g.Name).ToListAsync(ct);
        var daily = await query.GroupBy(e => e.ExpenseDate).Select(g => new { Date = g.Key, Total = g.Sum(e => e.Amount) }).OrderBy(g => g.Date).ToListAsync(ct);
        var monthly = await query.GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Amount) }).OrderBy(g => g.Year).ThenBy(g => g.Month).ToListAsync(ct);
        return new(total, count, highest == null ? null : ExpenseItem.From(highest), recent.Select(ExpenseItem.From).ToList(),
            categories.Select(c => new CategoryTotal(c.CategoryId, c.Name, c.Color, c.Total, c.Count)).ToList(),
            daily.Select(d => new DayTotal(d.Date, d.Total)).ToList(), monthly.Select(m => new MonthTotal(m.Year, m.Month, m.Total)).ToList());
    }
}
