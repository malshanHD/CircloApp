using CircloApp.Application.Features.Events.DTOs;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Domain.Enums;
using MediatR;

namespace CircloApp.Application.Features.Events.Commands
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, CreateEventRespose>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventMemberRepository _memberRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateEventCommandHandler(IEventRepository eventRepository, IEventMemberRepository eventMemberRepository,
                                         IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
        {
            _eventRepository = eventRepository;
            _memberRepository = eventMemberRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<CreateEventRespose> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            var budgetEvent = new BudgetEvent
            {
                Id = Guid.NewGuid(),
                Name = request.CreateEventRequest.Name,
                Description = request.CreateEventRequest.Description,
                CreatedByUserId = request.UserId,
                CreatedAt = _dateTimeProvider.UtcNow,
                UpdatedAt = _dateTimeProvider.UtcNow,
            };

            var member = new EventMember
            {
                Id = Guid.NewGuid(),
                EventId = budgetEvent.Id,
                UserId = request.UserId,
                Role = "Admin",
                JoinedAt = _dateTimeProvider.UtcNow,
                CreatedAt = _dateTimeProvider.UtcNow,
                UpdatedAt = _dateTimeProvider.UtcNow,
            };

            await _eventRepository.AddAsync(budgetEvent, cancellationToken);
            await _memberRepository.AddAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateEventRespose
            {
                EventId = budgetEvent.Id,
                Message = "Event Created Successfully"
            };
        }
    }
}
