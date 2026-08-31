using MediatR;

namespace CircloApp.Application.Features.AI.Queries.SmartQuery
{
    public record AskCircloSmartQuery(Guid EventId, string Question, CancellationToken CancellationToken) : IRequest<string>;
}
