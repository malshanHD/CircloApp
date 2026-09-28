using CircloApp.Application.Exceptions;

namespace CircloApp.Application.Features.PersonalExpenses.DTOs;

public class ExpenseFilter
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "ExpenseDate";
    public string SortDirection { get; set; } = "desc";

    // From is inclusive, To is exclusive. Explicit date ranges take precedence only
    // when year/month are omitted, avoiding ambiguous combined filters.
    public (DateOnly? From, DateOnly? To) ValidateAndGetRange()
    {
        if (Page < 1 || Page > 1000000 || PageSize < 1 || PageSize > 100)
            throw new BadRequestException("Page must be positive and page size must be between 1 and 100.");
        if (MinAmount < 0 || MaxAmount < 0 || MinAmount > MaxAmount)
            throw new BadRequestException("Invalid amount range.");
        SortBy = (SortBy ?? "ExpenseDate").Trim().ToLowerInvariant();
        SortDirection = (SortDirection ?? "desc").Trim().ToLowerInvariant();
        if (!new[] { "expensedate", "createdat", "amount", "description", "category" }.Contains(SortBy)
            || !new[] { "asc", "desc" }.Contains(SortDirection))
            throw new BadRequestException("Invalid sorting field or direction.");
        if (Search?.Length > 250 || PaymentMethod?.Length > 50)
            throw new BadRequestException("Filter text is too long.");
        if (FromDate.HasValue || ToDate.HasValue)
        {
            if (Year.HasValue || Month.HasValue || FromDate > ToDate || ToDate == DateOnly.MaxValue)
                throw new BadRequestException("Use either a month or a valid date range, not both.");
            return (FromDate, ToDate?.AddDays(1));
        }
        var now = DateTime.UtcNow;
        var year = Year ?? now.Year;
        var month = Month ?? now.Month;
        PersonalExpenseRules.ValidateMonth(year, month);
        var start = new DateOnly(year, month, 1);
        return (start, start.AddMonths(1));
    }
}

public static class PersonalExpenseRules
{
    public static void ValidateMonth(int year, int month)
    {
        if (year is < 1 or > 9998 || month is < 1 or > 12)
            throw new BadRequestException("Year must be 1–9998 and month must be 1–12.");
    }
}
