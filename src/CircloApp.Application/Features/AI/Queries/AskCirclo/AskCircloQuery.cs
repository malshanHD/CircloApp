using MediatR;

namespace CircloApp.Application.Features.AI.Queries.AskCirclo
{
    public record AskCircloQuery(Guid EventId, string Question) : IRequest<string>;
}
