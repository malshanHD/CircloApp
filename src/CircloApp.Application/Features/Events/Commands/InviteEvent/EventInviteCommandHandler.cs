using CircloApp.Application.Exceptions;
using MediatR;
namespace CircloApp.Application.Features.Events.Commands.InviteEvent;
// Replaced by shared links; do not create unsolicited pending memberships or send email.
public class EventInviteCommandHandler : IRequestHandler<EventInviteCommand, Guid>
{
    public Task<Guid> Handle(EventInviteCommand request, CancellationToken cancellationToken) =>
        throw new BadRequestException("Share the event link so the user can request to join.");
}
