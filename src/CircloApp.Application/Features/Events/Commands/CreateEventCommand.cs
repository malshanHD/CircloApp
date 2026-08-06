using CircloApp.Application.Features.Events.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Events.Commands
{
    public record CreateEventCommand(Guid UserId, CreateEventRequest CreateEventRequest) : IRequest<CreateEventRespose>;
}
