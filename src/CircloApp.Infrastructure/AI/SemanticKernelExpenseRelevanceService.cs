using CircloApp.Application.Features.AI_II.Models;
using CircloApp.Application.Interfaces.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using System.Text.Json;

namespace CircloApp.Infrastructure.AI
{
    public class SemanticKernelExpenseRelevanceService : IExpenseRelevanceService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;

        public SemanticKernelExpenseRelevanceService(IOptions<AzureAIOptionsII> options)
        {
            var aiOptions = options.Value;
            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(deploymentName: aiOptions.ChatDeploymentName, endpoint: aiOptions.Endpoint, apiKey: aiOptions.Apikey);

            _kernel = builder.Build();

            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }
        public async Task<List<Guid>> GetRelevantExpenseIdsAsync(string question, IReadOnlyList<ExpenseSearchResult> candidates, CancellationToken cancellationToken)
        {
            if (candidates.Count == 0) {
                return [];
            }

            var candidateBuild = new StringBuilder();

            foreach (var candidate in candidates)
            {
                candidateBuild.AppendLine($"""Id: {candidate.ExpenseId}, Description: {candidate.Description}""");
            }

            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage(
                """"
                You are an expense relevance classifier for Circlo,
                a group expense management application.

                Your job is NOT to calculate money.

                Your job is to decide which candidate expenses are
                semantically relevant to the user's question.

                Example:

                User question:
                "How much did we spend on transport?"

                Relevant examples:
                - Uber from airport
                - Taxi to hotel
                - Train ticket
                - Bus ticket
                - Fuel for rented bike
                - Bike rental

                Not relevant:
                - Chicken fried rice
                - Hotel room
                - Coffee
                - Museum ticket

                Rules:

                1. Only return expense IDs from the provided candidate list.
                2. Do not invent IDs.
                3. Only select expenses clearly related to the user's question.
                4. If nothing is relevant, return an empty array.
                5. Do not calculate totals.
                6. Return ONLY valid JSON.
                7. Do not include markdown or explanations.

                Return exactly this structure:

                {
                  "relevantExpenseIds": [
                    "expense-guid"
                  ]
                }
                """");

            chatHistory.AddUserMessage(
                $""""
                User question:

                {question}

                Candidate expenses:

                {candidateBuild}
                """");

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel, cancellationToken: cancellationToken);

            var content = response.Content;

            if (string.IsNullOrEmpty(content))
            {
                return [];
            }

            ExpenseSelectionResponse? selection;

            try
            {
                selection = JsonSerializer.Deserialize<ExpenseSelectionResponse>(
                        content,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
            }
            catch (JsonException)
            {
                return [];
            }

            if (selection?.RelevantExpenseIds is null)
            {
                return [];
            }

            var candidateIds = candidates.Select(x => x.ExpenseId).ToHashSet();

            var relevantIds = new List<Guid>();

            foreach (var idValue in selection.RelevantExpenseIds)
            {
                if (!Guid.TryParse(idValue, out var expenseId))
                {
                    continue;
                }

                // Important security / correctness check:
                // GPT can only select IDs that actually came
                // from Azure AI Search.
                if (!candidateIds.Contains(expenseId))
                {
                    continue;
                }

                relevantIds.Add(expenseId);
            }

            return relevantIds.Distinct().ToList();
        }
    }
}
