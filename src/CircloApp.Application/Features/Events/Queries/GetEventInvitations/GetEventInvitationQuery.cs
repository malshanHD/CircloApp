using CircloApp.Application.Features.Events.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Events.Queries.GetEventInvitations
{
    public record GetEventInvitationQuery : IRequest<GetEventInviteResponse>;
}
