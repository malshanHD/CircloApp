using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CircloApp.Infrastructure.Repositories;
public class PersonalBudgetRepository(ApplicationDbContext context) : IPersonalBudgetRepository
{
    public Task<PersonalExpenseSettings?> SettingsAsync(Guid user, bool tracking, CancellationToken ct)
    {
        var query = context.PersonalExpenseSettings.Where(s => s.UserId == user);
        return (tracking ? query : query.AsNoTracking().Where(s => !s.IsDeleted)).SingleOrDefaultAsync(ct);
    }
    public Task<MonthlyExpenseBudget?> BudgetAsync(Guid user, int year, int month, bool tracking, CancellationToken ct)
    {
        var query = context.MonthlyExpenseBudgets.Where(b => b.UserId == user && b.Year == year && b.Month == month);
        return (tracking ? query : query.AsNoTracking().Where(b => !b.IsDeleted)).SingleOrDefaultAsync(ct);
    }
    public async Task AddSettingsAsync(PersonalExpenseSettings settings, CancellationToken ct) => await context.PersonalExpenseSettings.AddAsync(settings, ct);
    public async Task AddBudgetAsync(MonthlyExpenseBudget budget, CancellationToken ct) => await context.MonthlyExpenseBudgets.AddAsync(budget, ct);
}
