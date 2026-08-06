using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Infrastructure.Persistence;

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
    }
}
