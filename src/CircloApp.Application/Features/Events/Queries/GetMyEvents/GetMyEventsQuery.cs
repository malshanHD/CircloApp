using CircloApp.Application.Features.Events.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Events.Queries.GetMyEvents
{
    public record GetMyEventsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResponse<EventSummaryDto>>;
}
