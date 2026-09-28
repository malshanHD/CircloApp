using CircloApp.Domain.Entities;
namespace CircloApp.Application.Interfaces;
public interface IPersonalBudgetRepository
{
    Task<PersonalExpenseSettings?> SettingsAsync(Guid user, bool tracking, CancellationToken ct);
    Task<MonthlyExpenseBudget?> BudgetAsync(Guid user, int year, int month, bool tracking, CancellationToken ct);
    Task AddSettingsAsync(PersonalExpenseSettings settings, CancellationToken ct);
    Task AddBudgetAsync(MonthlyExpenseBudget budget, CancellationToken ct);
}
