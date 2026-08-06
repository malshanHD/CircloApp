using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.Events.Queries.GetEventDetails
{
    public class GetEventDetailsQueryHandler : IRequestHandler<GetEventDetailsQuery, EventDetailsDto>
    {
        private readonly IEventRepository _eventRepo;
        private readonly ICurrentUserService _currentUserService;

        public GetEventDetailsQueryHandler(IEventRepository eventRepository, ICurrentUserService currentUserService)
        {
            _eventRepo = eventRepository;
            _currentUserService = currentUserService;
        }
        public async Task<EventDetailsDto> Handle(GetEventDetailsQuery request, CancellationToken cancellationToken)
        {
            var eventDetails = await _eventRepo.GetEventDetailsAsync(request.EventId, _currentUserService.UserId, cancellationToken);

            if(eventDetails == null)
            {
                throw new DirectoryNotFoundException("Event not found");
            }

            return new EventDetailsDto
            {
                Id = eventDetails.Id,
                Name = eventDetails.Name,
                Description = eventDetails.Description,
                CreatedAt = eventDetails.CreatedAt,

                Members = eventDetails.Members.Select(m => new MemberDto
                {
                    UserId = m.UserId,
                    Username = m.Username,
                    FullName = m.FullName,
                    Role = m.Role
                }).ToList()
            };
        }
    }
}
