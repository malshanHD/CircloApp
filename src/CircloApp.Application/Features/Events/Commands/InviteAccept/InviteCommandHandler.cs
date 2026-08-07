using CircloApp.Application.Exceptions;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Events.Commands.InviteAccept
{
    public class InviteCommandHandler : IRequestHandler<InviteAcceptCommand, string>
    {
        private readonly IEventMemberRepository _eventMemberRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeProvider _timeProvider;

        public InviteCommandHandler(IEventMemberRepository eventMember, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider)
        {
            _eventMemberRepository = eventMember;
            _currentUserService = currentUserService;
            _timeProvider = dateTimeProvider;
        }

        public async Task<string> Handle(InviteAcceptCommand request, CancellationToken cancellationToken)
        {
            var member = await _eventMemberRepository.GetEventMember(request.EventId, _currentUserService.UserId, cancellationToken);
            if (member == null)
                throw new BadRequestException("No invitation found for this event.");

            if (member.IsActive)
                throw new BadRequestException("You are already an active member of this event.");

            member.IsActive = true;
            member.UpdatedAt = _timeProvider.UtcNow;

            await _eventMemberRepository.AcceptInvite(_currentUserService.UserId, request.EventId, cancellationToken);

            return "Invitation accepted successfully.";
        }
    }
}
