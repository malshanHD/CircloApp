using CircloApp.Application.Features.Events.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Events.Commands.InviteEvent
{
    public record EventInviteCommand(Guid eventId, InviteRequest InviteRequest) : IRequest<Guid>;
}
