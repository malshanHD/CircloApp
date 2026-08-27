using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.Infrastructure.Repositories
{
    public class EventAiAnalysisRepository : IEventAiAnalysisRepository
    {
        private readonly ApplicationDbContext _context;
        public EventAiAnalysisRepository(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<EventAiAnalysis?> GetEventAiAnalysisAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.EventAiAnalyses.FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken);
        }

        public async Task SaveAsync(EventAiAnalysis eventAiAnalysis, CancellationToken cancellationToken = default)
        {
            var existing = await _context.EventAiAnalyses.FirstOrDefaultAsync(x => x.EventId == eventAiAnalysis.EventId, cancellationToken);

            if(existing is null)
            {
                await _context.AddAsync(eventAiAnalysis, cancellationToken);
            }
            else
            {
                existing.Summary = eventAiAnalysis.Summary;
                existing.DataHash = eventAiAnalysis.DataHash;
                existing.Model = eventAiAnalysis.Model;
                existing.UpdatedAt = eventAiAnalysis.UpdatedAt;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
