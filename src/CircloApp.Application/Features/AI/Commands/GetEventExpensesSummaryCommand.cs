using CircloApp.Application.Features.AI.DTO;
using MediatR;

namespace CircloApp.Application.Features.AI.Commands
{
    public record GetEventExpensesSummaryCommand(Guid EventId) : IRequest<CategorizedExpensesResponse>;
}
