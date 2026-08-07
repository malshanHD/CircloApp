using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CircloApp.Infrastructure.Repositories
{
    public class EventMemberRepository : IEventMemberRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public EventMemberRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(EventMember eventMember, CancellationToken cancellationToken)
        {
            await _applicationDbContext.EventMembers.AddAsync(eventMember, cancellationToken);
        }

        public async Task<bool> IsMemberExist(Guid eventID, Guid userId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.EventMembers.AnyAsync(m => m.EventId == eventID && m.UserId == userId);
        }
    }
}
