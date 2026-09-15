using CircloApp.Application.Features.Events.DTOs;
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
            return await _applicationDbContext.EventMembers.Include(e => e.User).Where(e => e.EventId == eventID).ToListAsync();
        }

        public async Task<int> GetEventParticipantCountAsync(Guid eventId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.EventMembers.Where(b => b.EventId == eventId).CountAsync(cancellationToken);
        }

        public async Task<bool> IsMemberExist(Guid eventID, Guid userId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.EventMembers.AnyAsync(m => m.EventId == eventID && m.UserId == userId);
        }
    }
}
