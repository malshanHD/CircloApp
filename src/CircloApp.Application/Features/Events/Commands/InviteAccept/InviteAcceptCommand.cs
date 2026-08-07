using MediatR;

namespace CircloApp.Application.Features.Events.Commands.InviteAccept
{
    public record InviteAcceptCommand(Guid EventId) : IRequest<string>;
}
