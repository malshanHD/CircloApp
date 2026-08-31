using MediatR;

namespace CircloApp.Application.Features.AI.Queries.RAG
{
    public record AskCircloRagQuery(Guid EventId, string question) : IRequest<string>;
}
