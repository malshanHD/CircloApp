using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IEventAiAnalysisRepository
    {
        Task<EventAiAnalysis?> GetEventAiAnalysisAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task SaveAsync(EventAiAnalysis eventAiAnalysis, CancellationToken cancellationToken = default);
    }
}
