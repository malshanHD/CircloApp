using CircloApp.Application.Features.Expenses.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Queries.GetEventExpenses
{
    public record GetEventExpensesQuery(Guid EventId) : IRequest<EventExpensesSummaryResponse>;
}
