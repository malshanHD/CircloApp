using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Events.Queries.GetMyEvents
{
    public class GetMyEventsQueryHandler : IRequestHandler<GetMyEventsQuery, PagedResponse<EventSummaryDto>>
    {
        private IEventRepository _eventRepository;
        private ICurrentUserService _currentUserService;

        public GetMyEventsQueryHandler(IEventRepository eventRepository, ICurrentUserService currentUserService)
        {
            _eventRepository = eventRepository;
            _currentUserService = currentUserService;
        }
        public async Task<PagedResponse<EventSummaryDto>> Handle(GetMyEventsQuery request, CancellationToken cancellationToken)
        {
            return await _eventRepository.GetMyEventsAsync(_currentUserService.UserId, request.Page, request.PageSize, cancellationToken);
        }
    }
}
