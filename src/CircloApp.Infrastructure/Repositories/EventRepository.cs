using CircloApp.Application.Features.Events.DTOs;
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

        public async Task<PagedResponse<EventSummaryDto>> GetMyEventsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = _context.EventMembers.AsNoTracking().Where(em => em.UserId == userId).Select(em => em.Event);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.OrderByDescending(e => e.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).Select(e => new EventSummaryDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
                MemberCount = e.Members.Count(m => m.IsActive),
                IsAdmin = e.Members.Any(m => m.UserId == userId && m.Role == "Admin" && m.IsActive)
            }).ToListAsync(cancellationToken);

            return new PagedResponse<EventSummaryDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<BudgetEvent>> GetUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.BudgetEvents.Where(x => x.CreatedByUserId == userId).ToListAsync(cancellationToken);
        }
    }
}
