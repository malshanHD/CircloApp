using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.PersonalExpenses.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;
namespace CircloApp.Application.Features.PersonalExpenses;
public record GetPersonalDashboard(Guid UserId, int? Year, int? Month) : IRequest<PersonalDashboard>;
public record GetPersonalAnalysis(Guid UserId, DateOnly FromDate, DateOnly ToDate) : IRequest<PersonalAnalysis>;
public class PersonalAnalyticsHandler(IPersonalAnalyticsRepository analytics, IPersonalBudgetRepository budgets) :
    IRequestHandler<GetPersonalDashboard, PersonalDashboard>, IRequestHandler<GetPersonalAnalysis, PersonalAnalysis>
{
    public async Task<PersonalDashboard> Handle(GetPersonalDashboard q, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int year = q.Year ?? today.Year, month = q.Month ?? today.Month;
        PersonalExpenseRules.ValidateMonth(year, month);
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);
        var s = await analytics.StatisticsAsync(q.UserId, from, to, ct);
        var settings = await budgets.SettingsAsync(q.UserId, false, ct);
        var budget = await budgets.BudgetAsync(q.UserId, year, month, false, ct);
        decimal? limit = budget?.LimitAmount ?? settings?.DefaultMonthlyLimit;
        var source = budget != null ? "MonthlyOverride" : settings != null ? "Default" : "NotConfigured";
        decimal? remaining = limit - s.TotalSpent;
        decimal? percentage = limit > 0 ? decimal.Round(s.TotalSpent / limit.Value * 100, 2)
            : limit == 0 && s.TotalSpent == 0 ? 0 : null;
        var status = limit == null ? "NotConfigured" : s.TotalSpent > limit ? "Exceeded" :
            percentage >= (settings?.WarningPercentage ?? 80) ? "NearLimit" : "OnTrack";
        // Current month uses elapsed days; completed/future months use their full length.
        var days = year == today.Year && month == today.Month ? today.Day : DateTime.DaysInMonth(year, month);
        var daily = Enumerable.Range(0, to.DayNumber - from.DayNumber)
            .Select(i => new DayTotal(from.AddDays(i), s.DailyTotals.FirstOrDefault(d => d.Date == from.AddDays(i))?.Total ?? 0)).ToList();
        return new(year, month, settings?.CurrencyCode ?? "LKR", limit, source, s.TotalSpent, remaining,
            Math.Max(0, -(remaining ?? 0)), percentage, s.ExpenseCount, decimal.Round(s.TotalSpent / days, 2),
            s.HighestExpense, s.RecentExpenses, Percentages(s), daily, status);
    }
    public async Task<PersonalAnalysis> Handle(GetPersonalAnalysis q, CancellationToken ct)
    {
        var days = q.ToDate.DayNumber - q.FromDate.DayNumber + 1;
        if (q.FromDate == default || days < 1 || days > 3660 || q.ToDate == DateOnly.MaxValue || q.FromDate.DayNumber < days)
            throw new BadRequestException("Select a valid date range of at most ten years with room for the preceding comparison period.");
        var s = await analytics.StatisticsAsync(q.UserId, q.FromDate, q.ToDate.AddDays(1), ct);
        var previous = await analytics.TotalAsync(q.UserId, q.FromDate.AddDays(-days), q.FromDate, ct);
        var settings = await budgets.SettingsAsync(q.UserId, false, ct);
        var categories = Percentages(s);
        return new(q.FromDate, q.ToDate, settings?.CurrencyCode ?? "LKR", s.TotalSpent, s.ExpenseCount,
            s.ExpenseCount == 0 ? 0 : decimal.Round(s.TotalSpent / s.ExpenseCount, 2), s.HighestExpense,
            categories, s.MonthlyTotals, categories.FirstOrDefault(), s.DailyTotals.OrderByDescending(d => d.Total).ThenBy(d => d.Date).FirstOrDefault(),
            previous, previous == 0 ? null : decimal.Round((s.TotalSpent - previous) / previous * 100, 2));
    }
    private static IReadOnlyList<CategoryTotal> Percentages(ExpenseStatistics s) => s.CategoryTotals
        .Select(c => c with { Percentage = s.TotalSpent == 0 ? 0 : decimal.Round(c.Total / s.TotalSpent * 100, 2) }).ToList();
}
