using CircloApp.Application.Features.AI_II.Models;
using CircloApp.Application.Interfaces.AI;
using MediatR;
using System.Text;

namespace CircloApp.Application.Features.AI_II.Queries.AskAi
{
    public class AskAiQueryHandler : IRequestHandler<AskAiQuery, string>
    {
        private readonly IAiChatService _chatService;
        private readonly IExpenseSearchService _expenseSearchService;
        private readonly IAiIntentService _aiIntentService;
        private readonly IExpenseRelevanceService _expenseRelevanceService;

        public AskAiQueryHandler(IAiChatService chatService, IExpenseSearchService expenseSearchService, 
                                 IAiIntentService aiIntentService, IExpenseRelevanceService expenseRelevanceService)
        {
            _chatService = chatService;
            _expenseSearchService = expenseSearchService;
            _aiIntentService = aiIntentService;
            _expenseRelevanceService = expenseRelevanceService;
        }

        public async Task<string> Handle(AskAiQuery request, CancellationToken cancellationToken)
        {
            var intent = await _aiIntentService.DetermineIntentAsync(request.Question, cancellationToken);

            if (intent == AiQueryIntent.Calculation)
            {
                var candidates = await _expenseSearchService.SearchExpensesAsync(request.EventId, request.Question, cancellationToken);

                var relevantExpenseIds = await _expenseRelevanceService.GetRelevantExpenseIdsAsync(request.Question, candidates, cancellationToken);

                return string.Join(Environment.NewLine, relevantExpenseIds);
            }

            var searchResults = await _expenseSearchService.SearchExpensesAsync(request.EventId, request.Question, cancellationToken);

            var contextBuilder = new StringBuilder();

            foreach (var result in searchResults)
            {
                contextBuilder.AppendLine($"Description: {result.Description}, Amount: {result.Amount}");
            }

            var prompt =
                $"""
                You are Circlo AI, an assistant for a group expense application.

                Answer the user's question using ONLY the expense data below.

                If the provided data is not enough to answer the question,
                say that you do not have enough information.

                Expense data:
                {contextBuilder}

                User question:
                {request.Question}
                """;

            return await _chatService.AskAsync(prompt, cancellationToken);
        }
    }
}
