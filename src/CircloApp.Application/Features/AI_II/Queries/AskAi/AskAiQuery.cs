using MediatR;

namespace CircloApp.Application.Features.AI_II.Queries.AskAi
{
    public record AskAiQuery(string Question) : IRequest<string>;
}
