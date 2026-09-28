using CircloApp.Application.Features.PersonalExpenses.DTOs;
namespace CircloApp.Application.Interfaces;
public interface IPersonalAnalyticsRepository
{
    Task<ExpenseStatistics> StatisticsAsync(Guid user, DateOnly from, DateOnly toExclusive, CancellationToken ct);
    Task<decimal> TotalAsync(Guid user, DateOnly from, DateOnly toExclusive, CancellationToken ct);
}
