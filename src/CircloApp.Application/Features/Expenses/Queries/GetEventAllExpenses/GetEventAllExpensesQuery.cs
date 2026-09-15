using CircloApp.Application.Features.Expenses.DTOs;
using MediatR;

namespace CircloApp.Application.Features.Expenses.Queries.GetEventAllExpenses
{
    public record GetEventAllExpensesQuery(Guid EventId) : IRequest<List<EventAllExpensesResponse>>;
}
