using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;
        public EventRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }
        public async Task AddAsync(BudgetEvent budgetEvent, CancellationToken cancellationToken)
        {
            await _context.BudgetEvents.AddAsync(budgetEvent, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.BudgetEvents.AnyAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<BudgetEvent> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.BudgetEvents.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<BudgetEvent>> GetUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.BudgetEvents.Where(x => x.CreatedByUserId == userId).ToListAsync(cancellationToken);
        }
    }
}
