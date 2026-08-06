using CircloApp.Application.Features.Events.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Events.Queries.GetEventDetails
{
    public record GetEventDetailsQuery(Guid EventId) : IRequest<EventDetailsDto>;
}
