using CircloApp.Application.Interfaces;
using MediatR;

namespace CircloApp.Application.Features.AI.Queries.AskCirclo
{
    public class AskCircloQueryHandler : IRequestHandler<AskCircloQuery, string>
    {
        private readonly IAiService _aiService;

        public AskCircloQueryHandler(IAiService aiService)
        {
            _aiService = aiService;
        }

        public async Task<string> Handle(AskCircloQuery request, CancellationToken cancellationToken)
        {
            return await _aiService.AskCircloAsync(request.EventId, request.Question, cancellationToken);
        }
    }
}
