using CircloApp.Application.Exceptions;
using CircloApp.Application.Features.Expenses.Commands.AddExpenses;
using CircloApp.Application.Features.Expenses.DTOs;
using CircloApp.Application.Features.Expenses.Queries.GetEventAllExpenses;
using CircloApp.Application.Features.Expenses.Queries.GetEventExpenses;
using CircloApp.Application.Interfaces;
using CircloApp.Application.Interfaces.AI;
using Moq;
namespace CircloApp.Application.Tests.Features.Events;
public class PendingMemberAccessTests
{
    [Fact] public async Task Pending_member_cannot_read_or_write_event_expenses()
    {
        var eventId = Guid.NewGuid();
        var current = Mock.Of<ICurrentUserService>();
        // IsMemberExist now tests active, nondeleted membership in the repository.
        var members = Mock.Of<IEventMemberRepository>();
        var expenses = new Mock<IExpensesService>(MockBehavior.Strict);
        var summary = new Mock<IEventExpenseSummaryHelper>(MockBehavior.Strict);
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var clock = Mock.Of<IDateTimeProvider>();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new GetEventAllExpensesQueryHandler(expenses.Object, current, members).Handle(new GetEventAllExpensesQuery(eventId), default));
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new GetEventExpensesQueryHandler(current, expenses.Object, clock, members, summary.Object).Handle(new GetEventExpensesQuery(eventId), default));
        await Assert.ThrowsAsync<BadRequestException>(() =>
            new AddExpensesCommandHandler(expenses.Object, current, clock, unit.Object, members, summary.Object, Mock.Of<IExpenseVectorSearchService>(), Mock.Of<IExpenseSearchIndexer>())
                .Handle(new AddExpensesCommand(new CreateExpensesRequest { Amount=10, Description="Test" }, eventId), default));
        expenses.VerifyNoOtherCalls(); summary.VerifyNoOtherCalls(); unit.VerifyNoOtherCalls();
    }
}

