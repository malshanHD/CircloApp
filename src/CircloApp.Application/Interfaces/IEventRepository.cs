using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Application.QueryModels.Events;
using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IEventRepository
    {
        Task AddAsync(BudgetEvent budgetEvent, CancellationToken cancellationToken);
        Task<BudgetEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<BudgetEvent>> GetUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
        Task<PagedResponse<EventSummaryDto>> GetMyEventsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
        Task<EventSummaryModel?> GetEventDetailsAsync(Guid eventId, Guid currentUser, CancellationToken cancellationToken);
    }
}
