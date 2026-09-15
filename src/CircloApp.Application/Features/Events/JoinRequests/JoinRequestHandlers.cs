using CircloApp.Application.Exceptions;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using CircloApp.Domain.Enums;
using MediatR;

namespace CircloApp.Application.Features.Events.JoinRequests;

public record JoinStatus(string EventName, string Status);
public record PendingJoinRequest(Guid EventId, Guid UserId, string EventName, string FullName, string Username);
public record GetJoinStatusQuery(Guid EventId) : IRequest<JoinStatus>;
public record RequestJoinCommand(Guid EventId) : IRequest<JoinStatus>;
public record ApproveJoinCommand(Guid EventId, Guid UserId) : IRequest<string>;
public record GetJoinRequestsQuery : IRequest<List<PendingJoinRequest>>;

public class JoinRequestHandlers(IEventRepository events, IEventMemberRepository members,
    IUserRepository users, ICurrentUserService currentUser, IDateTimeProvider clock) :
    IRequestHandler<GetJoinStatusQuery, JoinStatus>,
    IRequestHandler<RequestJoinCommand, JoinStatus>,
    IRequestHandler<ApproveJoinCommand, string>,
    IRequestHandler<GetJoinRequestsQuery, List<PendingJoinRequest>>
{
    private async Task<BudgetEvent> ExistingEvent(Guid eventId, CancellationToken ct)
    {
        if (!await users.ExistsByIdAsync(currentUser.UserId, ct))
            throw new BadRequestException("An existing Circlo account is required.");
        var budgetEvent = await events.GetByIdAsync(eventId, ct);
        if (budgetEvent is null || budgetEvent.IsDeleted)
            throw new BadRequestException("Event not found.");
        return budgetEvent;
    }

    public async Task<JoinStatus> Handle(GetJoinStatusQuery request, CancellationToken ct)
    {
        var budgetEvent = await ExistingEvent(request.EventId, ct);
        var member = await members.GetEventMember(request.EventId, currentUser.UserId, ct);
        if (member?.IsDeleted == true) throw new BadRequestException("This membership is no longer available. Contact the event admin.");
        return new(budgetEvent.Name, member is null ? "not-requested" : member.IsActive ? "active" : "pending");
    }

    public async Task<JoinStatus> Handle(RequestJoinCommand request, CancellationToken ct)
    {
        var budgetEvent = await ExistingEvent(request.EventId, ct);
        var member = await members.EnsureJoinRequestAsync(new EventMember
        {
            Id = Guid.NewGuid(), EventId = request.EventId, UserId = currentUser.UserId,
            Role = EventMemberRole.Membber, IsActive = false,
            JoinedAt = clock.UtcNow, CreatedAt = clock.UtcNow, UpdatedAt = clock.UtcNow
        }, ct);
        if (member.IsDeleted) throw new BadRequestException("This membership is no longer available. Contact the event admin.");
        return new(budgetEvent.Name, member.IsActive ? "active" : "pending");
    }

    public async Task<string> Handle(ApproveJoinCommand request, CancellationToken ct)
    {
        await ExistingEvent(request.EventId, ct);
        if (!await events.IsEventCreatedByUserAsync(request.EventId, currentUser.UserId, ct))
            throw new BadRequestException("Only the event admin can approve join requests.");
        var member = await members.GetEventMember(request.EventId, request.UserId, ct);
        if (member is null || member.IsDeleted || !await users.ExistsByIdAsync(request.UserId, ct))
            throw new BadRequestException("Join request not found.");
        if (!member.IsActive)
            await members.ApproveJoinRequestAsync(request.EventId, request.UserId, clock.UtcNow, ct);
        return "Join request approved.";
    }

    public async Task<List<PendingJoinRequest>> Handle(GetJoinRequestsQuery request, CancellationToken ct)
    {
        if (!await users.ExistsByIdAsync(currentUser.UserId, ct))
            throw new BadRequestException("An existing Circlo account is required.");
        return await members.GetPendingJoinRequestsAsync(currentUser.UserId, ct);
    }
}
