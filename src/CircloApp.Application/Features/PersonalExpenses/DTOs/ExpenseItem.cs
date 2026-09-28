using CircloApp.Domain.Entities;
namespace CircloApp.Application.Features.PersonalExpenses.DTOs;

public record ExpenseItem(Guid Id, string Description, decimal Amount, DateOnly ExpenseDate,
    Guid? CategoryId, string CategoryName, string? CategoryColor, string? CategoryIcon,
    string? PaymentMethod, string? Note, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static ExpenseItem From(PersonalExpense e) => new(e.Id, e.Description, e.Amount,
        e.ExpenseDate, e.CategoryId, e.Category?.Name ?? "Uncategorized", e.Category?.Color,
        e.Category?.Icon, e.PaymentMethod, e.Note, e.CreatedAt, e.UpdatedAt);
}
public record ExpensePage(IReadOnlyList<ExpenseItem> Items, int Page, int PageSize, int TotalCount, int TotalPages);
