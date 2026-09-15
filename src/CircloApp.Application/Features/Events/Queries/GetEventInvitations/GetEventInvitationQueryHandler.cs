using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Events.Queries.GetEventInvitations
{
    public class GetEventInvitationQueryHandler : IRequestHandler<GetEventInvitationQuery, GetEventInviteResponse>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IEventMemberRepository _eventMemberRepository;

        public GetEventInvitationQueryHandler(ICurrentUserService currentUserService, IEventMemberRepository eventMemberRepository)
        {
            _currentUserService = currentUserService;
            _eventMemberRepository = eventMemberRepository;
        }

        public async Task<GetEventInviteResponse> Handle(GetEventInvitationQuery request, CancellationToken cancellationToken)
        {
            return await _eventMemberRepository.GetEventInvitations(_currentUserService.UserId, cancellationToken);
        }
    }
}
