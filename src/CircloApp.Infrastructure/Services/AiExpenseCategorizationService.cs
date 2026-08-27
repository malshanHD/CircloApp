using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.Infrastructure.Services
{
    public class AiExpenseCategorizationService : IAiExpenseCategorizationService
    {
        private readonly ApplicationDbContext _context;

        public AiExpenseCategorizationService(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task AddRangeAsync(IEnumerable<ExpenseAiCategory> categories, CancellationToken cancellationToken = default)
        {
            await _context.ExpenseAiCategories.AddRangeAsync(categories, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<ExpenseAiCategory>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.ExpenseAiCategories.AsNoTracking()
                                                        .Where(x => x.Expense.EventId == eventId)
                                                        .ToListAsync(cancellationToken);
        }
    }
}
