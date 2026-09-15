using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IEventMemberRepository
    {
        Task<EventMember> EnsureJoinRequestAsync(EventMember member, CancellationToken cancellationToken);
        Task ApproveJoinRequestAsync(Guid eventId, Guid userId, DateTime approvedAt, CancellationToken cancellationToken);
        Task<List<CircloApp.Application.Features.Events.JoinRequests.PendingJoinRequest>> GetPendingJoinRequestsAsync(Guid adminId, CancellationToken cancellationToken);
        Task AddAsync(EventMember eventMember, CancellationToken cancellationToken);
        Task<bool> IsMemberExist(Guid eventID, Guid userId, CancellationToken cancellationToken);
        Task AcceptInvite(Guid userId, Guid eventId, CancellationToken cancellationToken);
        Task<EventMember> GetEventMember(Guid eventID, Guid userId, CancellationToken cancellationToken);
        Task<int> GetEventParticipantCountAsync(Guid eventId, CancellationToken cancellationToken);
        Task<List<EventMember>> GetEventMembers(Guid eventID, CancellationToken cancellationToken);
        Task<GetEventInviteResponse> GetEventInvitations(Guid userId, CancellationToken cancellationToken);
    }
}
