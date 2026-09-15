using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Events.JoinRequests;
using CircloApp.Application.Features.Events.Commands.InviteAccept;
using CircloApp.Application.Interfaces;
using CircloApp.Domain.Entities;
using Moq;
namespace CircloApp.Application.Tests.Features.Events;
public class JoinRequestTests
{
    private readonly Guid eventId = Guid.NewGuid(), actorId = Guid.NewGuid(), memberId = Guid.NewGuid();
    private readonly Mock<IEventRepository> events = new();
    private readonly Mock<IEventMemberRepository> members = new();
    private readonly Mock<IUserRepository> users = new();
    private readonly Mock<ICurrentUserService> current = new();
    private readonly Mock<IDateTimeProvider> clock = new();
    private JoinRequestHandlers Handler() => new(events.Object, members.Object, users.Object, current.Object, clock.Object);
    public JoinRequestTests()
    {
        current.SetupGet(x => x.UserId).Returns(actorId);
        clock.SetupGet(x => x.UtcNow).Returns(new DateTime(2026,9,15,0,0,0,DateTimeKind.Utc));
        users.Setup(x => x.ExistsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        events.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(new BudgetEvent { Id=eventId, Name="Trip" });
    }
    [Fact] public async Task Join_creates_inactive_member_for_authenticated_user_only()
    {
        members.Setup(x => x.EnsureJoinRequestAsync(It.IsAny<EventMember>(), It.IsAny<CancellationToken>())).ReturnsAsync((EventMember m, CancellationToken _) => m);
        var result = await Handler().Handle(new RequestJoinCommand(eventId), default);
        Assert.Equal("pending", result.Status);
        members.Verify(x => x.EnsureJoinRequestAsync(It.Is<EventMember>(m => m.UserId == actorId && m.EventId == eventId && !m.IsActive && m.Id != Guid.Empty && m.Role != "Admin"), It.IsAny<CancellationToken>()), Times.Once);
        members.Verify(x => x.ApproveJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact] public async Task Missing_account_cannot_request_membership()
    {
        users.Setup(x => x.ExistsByIdAsync(actorId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        await Assert.ThrowsAsync<BadRequestException>(() => Handler().Handle(new RequestJoinCommand(eventId), default));
        members.VerifyNoOtherCalls();
    }
    [Fact] public async Task Missing_event_cannot_create_membership()
    {
        events.Setup(x => x.GetByIdAsync(eventId, It.IsAny<CancellationToken>())).ReturnsAsync((BudgetEvent)null!);
        await Assert.ThrowsAsync<BadRequestException>(() => Handler().Handle(new RequestJoinCommand(eventId), default));
        members.VerifyNoOtherCalls();
    }
    [Theory] [InlineData(false, "pending")] [InlineData(true, "active")]
    public async Task Repeat_request_preserves_existing_status(bool active, string status)
    {
        members.Setup(x => x.EnsureJoinRequestAsync(It.IsAny<EventMember>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EventMember { IsActive=active });
        Assert.Equal(status, (await Handler().Handle(new RequestJoinCommand(eventId), default)).Status);
    }
    [Fact] public async Task Non_admin_cannot_approve_even_their_own_request()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => Handler().Handle(new ApproveJoinCommand(eventId, actorId), default));
        members.VerifyNoOtherCalls();
    }
    [Fact] public async Task Admin_approves_exact_pending_member()
    {
        events.Setup(x => x.IsEventCreatedByUserAsync(eventId, actorId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        members.Setup(x => x.GetEventMember(eventId, memberId, It.IsAny<CancellationToken>())).ReturnsAsync(new EventMember());
        await Handler().Handle(new ApproveJoinCommand(eventId, memberId), default);
        members.Verify(x => x.ApproveJoinRequestAsync(eventId, memberId, clock.Object.UtcNow, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact] public async Task Admin_cannot_approve_missing_request()
    {
        events.Setup(x => x.IsEventCreatedByUserAsync(eventId, actorId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        await Assert.ThrowsAsync<BadRequestException>(() => Handler().Handle(new ApproveJoinCommand(eventId, memberId), default));
        members.Verify(x => x.ApproveJoinRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Fact] public async Task Requests_are_scoped_to_current_admin()
    {
        members.Setup(x => x.GetPendingJoinRequestsAsync(actorId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        Assert.Empty(await Handler().Handle(new GetJoinRequestsQuery(), default));
        members.Verify(x => x.GetPendingJoinRequestsAsync(actorId, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact] public async Task Legacy_accept_cannot_activate_a_membership() =>
        await Assert.ThrowsAsync<BadRequestException>(() => new InviteCommandHandler().Handle(new InviteAcceptCommand(eventId), default));
}
