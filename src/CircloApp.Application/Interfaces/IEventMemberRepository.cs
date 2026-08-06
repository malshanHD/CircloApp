using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IEventMemberRepository
    {
        Task AddAsync(EventMember eventMember, CancellationToken cancellationToken);
    }
}
