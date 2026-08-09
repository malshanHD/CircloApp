using CircloApp.Application.Features.Expenses.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Commands.AddExpenses
{
    public record AddExpensesCommand(CreateExpensesRequest CreateExpenses, Guid EventId) : IRequest<EventExpensesSummaryResponse>;
}
