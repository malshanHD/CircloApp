using CircloApp.Application.Features.Expenses.DTOs;

namespace CircloApp.Application.Interfaces
{
    public interface IEventExpenseSummaryHelper
    {
        Task<EventExpensesSummaryResponse> EventExpensesSummaryAsync(Guid eventId, CancellationToken cancellationToken);
    }
}
