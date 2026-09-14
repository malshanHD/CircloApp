using MediatR;

namespace CircloApp.Application.Features.AI_II.Queries.AskAi
{
    public record AskAiQuery(Guid EventId, string Question) : IRequest<string>;
}
