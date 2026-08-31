using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.AI.Queries.RAG
{
    public class AskCircloRagQueryHandler : IRequestHandler<AskCircloRagQuery, string>
    {
        private readonly IAiService _aiService;

        public AskCircloRagQueryHandler(IAiService aiService)
        {
            _aiService = aiService;
        }

        public async Task<string> Handle(AskCircloRagQuery request, CancellationToken cancellationToken)
        {
            return await _aiService.AskWithRagAsync(request.EventId, request.question, cancellationToken);
        }
    }
}
