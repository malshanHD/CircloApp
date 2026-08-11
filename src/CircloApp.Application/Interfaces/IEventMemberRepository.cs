using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IEventMemberRepository
    {
        Task AddAsync(EventMember eventMember, CancellationToken cancellationToken);
        Task<bool> IsMemberExist(Guid eventID, Guid userId, CancellationToken cancellationToken);
        Task AcceptInvite(Guid userId, Guid eventId, CancellationToken cancellationToken);
        Task<EventMember> GetEventMember(Guid eventID, Guid userId, CancellationToken cancellationToken);
        Task<int> GetEventParticipantCountAsync(Guid eventId, CancellationToken cancellationToken);
        Task<List<EventMember>> GetEventMembers(Guid eventID, CancellationToken cancellationToken);
    }
}
