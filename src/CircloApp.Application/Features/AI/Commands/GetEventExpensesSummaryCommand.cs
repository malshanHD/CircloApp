using MediatR;

namespace CircloApp.Application.Features.AI.Commands
{
    public record GetEventExpensesSummaryCommand(Guid EventId) : IRequest<string>;
}
