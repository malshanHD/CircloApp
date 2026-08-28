using CircloApp.Application.Features.AI.DTO;
using CircloApp.Application.Features.AI.Queries.GetEventAiAnalysis;
using CircloApp.Application.Features.Expenses.DTOs;

namespace CircloApp.Application.Interfaces
{
    public interface IAiService
    {
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
        Task<CategorizedExpensesResponse> CategorizedExpensesAsync(List<EventExpensesResponnse> expenses, CancellationToken cancellationToken = default);
        Task<string> GenerateExpenseSummaryAsync(decimal totalExpense, List<CategorySummaryDto> categories, CancellationToken cancellationToken = default);
        Task<string> AskCircloAsync(Guid eventId, string question, CancellationToken cancellationToken = default);
    }
}
