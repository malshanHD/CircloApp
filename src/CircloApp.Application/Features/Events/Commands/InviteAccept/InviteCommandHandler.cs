using CircloApp.Application.Exceptions;
using MediatR;
namespace CircloApp.Application.Features.Events.Commands.InviteAccept;
// Kept for internal compatibility; self-activation is no longer supported.
public class InviteCommandHandler : IRequestHandler<InviteAcceptCommand, string>
{
    public Task<string> Handle(InviteAcceptCommand request, CancellationToken cancellationToken) =>
        throw new BadRequestException("An event admin must approve your join request.");
}
