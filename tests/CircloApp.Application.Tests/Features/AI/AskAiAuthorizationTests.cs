using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.AI_II.Models;
using CircloApp.Application.Features.AI_II.Queries.AskAi;
using CircloApp.Application.Interfaces;
using CircloApp.Application.Interfaces.AI;
using CircloApp.Domain.Entities;
using Moq;

namespace CircloApp.Application.Tests.Features.AI;

public class AskAiAuthorizationTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public async Task Denied_members_never_reach_ai_or_search(bool exists, bool active, bool deleted)
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        var members = new Mock<IEventMemberRepository>();
        members.Setup(x => x.GetEventMember(eventId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists ? new EventMember { IsActive = active, IsDeleted = deleted } : null!);
        var chat = new Mock<IAiChatService>(MockBehavior.Strict);
        var search = new Mock<IExpenseSearchService>(MockBehavior.Strict);
        var intent = new Mock<IAiIntentService>(MockBehavior.Strict);
        var relevance = new Mock<IExpenseRelevanceService>(MockBehavior.Strict);
        var handler = new AskAiQueryHandler(chat.Object, search.Object, intent.Object, relevance.Object, members.Object, currentUser.Object, Mock.Of<IExpensesService>());

        var error = await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new AskAiQuery(eventId, "Summarize"), CancellationToken.None));
        Assert.Equal("Event not found", error.Message);
        chat.VerifyNoOtherCalls();
        search.VerifyNoOtherCalls();
        intent.VerifyNoOtherCalls();
        relevance.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Active_member_search_is_scoped_to_requested_event()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(userId);
        var members = new Mock<IEventMemberRepository>();
        members.Setup(x => x.GetEventMember(eventId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(new EventMember { IsActive = true });
        var chat = new Mock<IAiChatService>();
        chat.Setup(x => x.AskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("An event summary");
        var search = new Mock<IExpenseSearchService>();
        search.Setup(x => x.SearchExpensesAsync(eventId, "Summarize", It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<ExpenseSearchResult>());
        var intent = new Mock<IAiIntentService>();
        intent.Setup(x => x.AnalyzeAsync("Summarize", It.IsAny<CancellationToken>())).ReturnsAsync(new AiQueryAnalysis(AiQueryIntent.Semantic, default));
        var handler = new AskAiQueryHandler(chat.Object, search.Object, intent.Object, Mock.Of<IExpenseRelevanceService>(), members.Object, currentUser.Object, Mock.Of<IExpensesService>());

        Assert.Equal("I couldn't find any expenses clearly related to that question.", await handler.Handle(new AskAiQuery(eventId, "Summarize"), CancellationToken.None));
        search.Verify(x => x.SearchExpensesAsync(eventId, "Summarize", It.IsAny<CancellationToken>()), Times.Once);
        search.VerifyNoOtherCalls();
    }
}
