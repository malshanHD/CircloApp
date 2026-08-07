using CircloApp.Application.Exceptions;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Domain.Enums;
using MediatR;

namespace CircloApp.Application.Features.Events.Commands.InviteEvent
{
    public class EventInviteCommandHandler : IRequestHandler<EventInviteCommand, Guid>
    {
        private readonly IEventMemberRepository _eventMemberRepository;
        private readonly IEventRepository _eventRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailService _emailService;

        public EventInviteCommandHandler(IEventMemberRepository eventMember, IEventRepository eventRepository, ICurrentUserService currentUserService, 
                                         IUnitOfWork unitOfWork, IUserRepository userRepository, IDateTimeProvider dateTimeProvider, IEmailService emailService)
        {
            _eventMemberRepository = eventMember;
            _eventRepository = eventRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _dateTimeProvider = dateTimeProvider;
            _emailService = emailService;
        }

        public async Task<Guid> Handle(EventInviteCommand request, CancellationToken cancellationToken)
        {
            var eventResult = await _eventRepository.IsEventCreatedByUserAsync(request.eventId, _currentUserService.UserId, cancellationToken);
            if (!eventResult)
                throw new BadRequestException("Event not found");

            var user = await _userRepository.GetByUsernameOrEmailAsync(request.InviteRequest.Username);
            if (user is null)
            {
                throw new BadRequestException("Ïnvalid User");
            }

            var isUserExist = await _eventMemberRepository.IsMemberExist(request.eventId, user.Id, cancellationToken);
            if (isUserExist)
                throw new BadRequestException("This user already a member");

            var member = new EventMember
            {
                Id = new Guid(),
                EventId = request.eventId,
                UserId = user.Id,
                Role = EventMemberRole.Membber,
                JoinedAt = _dateTimeProvider.UtcNow,
                CreatedAt = _dateTimeProvider.UtcNow,
                UpdatedAt = _dateTimeProvider.UtcNow,
                IsActive = false
            };

            await _eventMemberRepository.AddAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            SendInvitationEmail(user.Email, user.FirstName, request.eventId);
            return member.Id;
        }

        private async Task SendInvitationEmail(string email, string name, Guid eventId)
        {
            // 1. Generate invitation token / payload
            var inviteToken = Guid.NewGuid().ToString("N");

            // 2. Build the secure frontend invitation link
            var baseUrl = "localhost"; // e.g., "https://app.circlo.com"
            var inviteUrl = $"{baseUrl}/accept-invite?eventId={eventId}&token={inviteToken}";

            string subject = "You're invited to join an event!";
            string bodyHtml = $"""
            <h2>Hi {name} You've been invited!</h2>
            <p>You have been invited to collaborate on an event.</p>
            <p><a href="{inviteUrl}" style="padding:10px 15px; background-color:#007bff; color:#fff; text-decoration:none; border-radius:5px;">Accept Invitation</a></p>
            <p>Or copy this link: {inviteUrl}</p>
            """;

            await _emailService.SendEmailNotification(email, subject, bodyHtml);
        }
    }
}
