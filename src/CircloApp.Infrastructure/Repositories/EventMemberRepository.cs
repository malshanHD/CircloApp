using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Application.Features.Events.JoinRequests;
using Microsoft.Data.SqlClient;
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

        public async Task<EventMember> EnsureJoinRequestAsync(EventMember member, CancellationToken ct)
        {
            var existing = await GetEventMember(member.EventId, member.UserId, ct);
            if (existing is not null) return existing;
            _applicationDbContext.EventMembers.Add(member);
            try { await _applicationDbContext.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
                // The unique event/user index arbitrates simultaneous clicks or tabs.
                _applicationDbContext.Entry(member).State = EntityState.Detached;
                var concurrent = await GetEventMember(member.EventId, member.UserId, ct);
                if (concurrent is null) throw;
                return concurrent;
            }
            return member;
        }

        public Task ApproveJoinRequestAsync(Guid eventId, Guid userId, DateTime approvedAt, CancellationToken ct) =>
            _applicationDbContext.EventMembers
                .Where(m => m.EventId == eventId && m.UserId == userId && !m.IsDeleted && !m.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsActive, true)
                    .SetProperty(m => m.JoinedAt, approvedAt).SetProperty(m => m.UpdatedAt, approvedAt), ct);

        public Task<List<PendingJoinRequest>> GetPendingJoinRequestsAsync(Guid adminId, CancellationToken ct) =>
            _applicationDbContext.EventMembers.AsNoTracking()
                .Where(m => !m.IsActive && !m.IsDeleted && !m.Event.IsDeleted && !m.User.IsDeleted && m.Event.CreatedByUserId == adminId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new PendingJoinRequest(m.EventId, m.UserId, m.Event.Name,
                    m.User.FirstName + " " + m.User.LastName, m.User.Username)).ToListAsync(ct);
        public async Task AcceptInvite(Guid userId, Guid eventId, CancellationToken cancellationToken)
        {
            var member = await _applicationDbContext.EventMembers
                                    .Where(m => m.EventId == eventId && m.UserId == userId)
                                        .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsActive, true), cancellationToken);
        }

        public async Task AddAsync(EventMember eventMember, CancellationToken cancellationToken)
        {
            await _applicationDbContext.EventMembers.AddAsync(eventMember, cancellationToken);
        }

        public async Task<GetEventInviteResponse> GetEventInvitations(Guid userId, CancellationToken cancellationToken)
        {
            var invitations = await _applicationDbContext.EventMembers.Where(m => m.UserId == userId && !m.IsActive)
                .Include(m => m.Event)
                .ThenInclude(e => e.CreatedByUser)
                .Select(m => new GetEventInviteResponseList
                {
                    EventId = m.EventId,
                    InviterName = m.Event.CreatedByUser.FirstName + " " + m.Event.CreatedByUser.LastName,
                    EventName = m.Event.Name
                })
                .ToListAsync(cancellationToken);

            var response = new GetEventInviteResponse
            {
                InvitationsCount = invitations.Count,
                InviteDetails = invitations
            };

            return response;
        }

        public async Task<EventMember> GetEventMember(Guid eventID, Guid userId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.EventMembers.FirstOrDefaultAsync(m => m.EventId == eventID && m.UserId == userId, cancellationToken: cancellationToken);
        }

        public async Task<List<EventMember>> GetEventMembers(Guid eventID, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.EventMembers.Include(e => e.User).Where(e => e.EventId == eventID && e.IsActive && !e.IsDeleted).ToListAsync(cancellationToken);
        }

        public async Task<int> GetEventParticipantCountAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.EventMembers.Where(b => b.EventId == eventId && b.IsActive && !b.IsDeleted).CountAsync(cancellationToken);
        }

        public async Task<bool> IsMemberExist(Guid eventID, Guid userId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.EventMembers.AnyAsync(m => m.EventId == eventID && m.UserId == userId && m.IsActive && !m.IsDeleted, cancellationToken);
        }
    }
}
