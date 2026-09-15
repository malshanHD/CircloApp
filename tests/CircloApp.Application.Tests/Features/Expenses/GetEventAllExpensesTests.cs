using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Features.Expenses.Queries.GetEventAllExpenses;
using CircloApp.Application.Interfaces;
using Moq;

namespace CircloApp.Application.Tests.Features.Expenses;

public class GetEventAllExpensesTests
{
    [Fact]
    public async Task Event_without_expenses_returns_an_empty_list()
    {
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var current = new Mock<ICurrentUserService>();
        current.SetupGet(x => x.UserId).Returns(userId);
        var members = new Mock<IEventMemberRepository>();
        members.Setup(x => x.IsMemberExist(eventId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var expenses = new Mock<IExpensesService>();
        expenses.Setup(x => x.GetEventExpenses(eventId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<EventExpensesResponnse>());
        var handler = new GetEventAllExpensesQueryHandler(expenses.Object, current.Object, members.Object);
        Assert.Empty(await handler.Handle(new GetEventAllExpensesQuery(eventId), CancellationToken.None));
    }
}