namespace CircloApp.Application.Features.PersonalExpenses.DTOs;
public record CategoryTotal(Guid? CategoryId, string Name, string? Color, decimal Total, int Count, decimal Percentage = 0);
public record DayTotal(DateOnly Date, decimal Total);
public record MonthTotal(int Year, int Month, decimal Total);
public record ExpenseStatistics(decimal TotalSpent, int ExpenseCount, ExpenseItem? HighestExpense,
    IReadOnlyList<ExpenseItem> RecentExpenses, IReadOnlyList<CategoryTotal> CategoryTotals,
    IReadOnlyList<DayTotal> DailyTotals, IReadOnlyList<MonthTotal> MonthlyTotals);
public record PersonalDashboard(int Year, int Month, string CurrencyCode, decimal? EffectiveMonthlyLimit,
    string LimitSource, decimal TotalSpent, decimal? RemainingBalance, decimal OverBudgetAmount,
    decimal? PercentageUsed, int ExpenseCount, decimal AverageDailySpending, ExpenseItem? LargestExpense,
    IReadOnlyList<ExpenseItem> RecentExpenses, IReadOnlyList<CategoryTotal> CategoryBreakdown,
    IReadOnlyList<DayTotal> DailySpendingTrend, string BudgetStatus);
public record PersonalAnalysis(DateOnly FromDate, DateOnly ToDate, string CurrencyCode, decimal TotalSpent,
    int ExpenseCount, decimal AverageExpenseAmount, ExpenseItem? HighestExpense,
    IReadOnlyList<CategoryTotal> CategoryTotals, IReadOnlyList<MonthTotal> MonthlyTotals,
    CategoryTotal? HighestSpendingCategory, DayTotal? HighestSpendingDay,
    decimal PreviousPeriodTotal, decimal? PercentageChange);
