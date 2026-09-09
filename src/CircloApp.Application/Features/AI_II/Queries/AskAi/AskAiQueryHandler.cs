using CircloApp.Application.Interfaces.AI;
using MediatR;

namespace CircloApp.Application.Features.AI_II.Queries.AskAi
{
    public class AskAiQueryHandler : IRequestHandler<AskAiQuery, string>
    {
        private readonly IAiChatService _chatService;
        public AskAiQueryHandler(IAiChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<string> Handle(AskAiQuery request, CancellationToken cancellationToken)
        {
            return await _chatService.AskAsync(request.Question, cancellationToken);
        }
    }
}
