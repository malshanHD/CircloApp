using CircloApp.Application.Features.AI_II.Models;
using CircloApp.Application.Interfaces.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace CircloApp.Infrastructure.AI
{
    public sealed class SemanticKernelAiIntentService : IAiIntentService
    {
        private readonly Kernel _kernel;    
        private readonly IChatCompletionService _chatCompletionService;

        public SemanticKernelAiIntentService(IOptions<AzureAIOptionsII> options)
        {
            var aiOptions = options.Value;
            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(deploymentName: aiOptions.ChatDeploymentName, endpoint: aiOptions.Endpoint, apiKey: aiOptions.Apikey);
            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<AiQueryAnalysis> AnalyzeAsync(string question, CancellationToken cancellationToken)
        {
            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage(
                """
                You are a query-routing classifier for Circlo,
                a group expense management application.

                Your job is only to determine how the backend should
                process the user's question.

                Do not answer the question.
                Do not calculate any monetary values.

                Available intents:

                General:
                Use for greetings, casual conversation, or questions
                about what Circlo AI can do.

                Examples:
                - "Hello"
                - "Who are you?"
                - "What can you do?"

                Semantic:
                Use when the user wants to find or list expenses based
                on the meaning of their descriptions.

                Examples:
                - "Show the transport expenses"
                - "Which expenses were related to food?"
                - "List our accommodation expenses"

                SemanticCalculation:
                Use when the user wants a numeric calculation over
                expenses that must first be selected by understanding
                their descriptions.

                Examples:
                - "How much did we spend on transport?"
                - "What was the food total?"
                - "How much was spent on accommodation?"

                StructuredCalculation:
                Use when the question can be answered from structured
                SQL fields without interpreting expense descriptions.

                Examples:
                - "Who paid most?"
                - "Who paid the highest amount?"
                - "What is the total event expense?"
                - "How much did each person pay?"

                Available operations:

                None:
                Use when no structured calculation operation is required.

                TopPayer:
                Use when the user asks who paid the most or highest amount.

                EventTotal:
                Use when the user asks for the total of all event expenses.

                PerPersonTotals:
                Use when the user asks how much each person paid.

                Rules:

                1. "Who paid most?" must be:
                   StructuredCalculation and TopPayer.

                2. A total for a semantic category such as food,
                   transport, or accommodation must be:
                   SemanticCalculation and None.

                3. Listing expenses from a semantic category must be:
                   Semantic and None.

                4. Greetings must be:
                   General and None.

                5. Return only valid JSON.

                6. Do not include markdown, explanations, or comments.

                Return exactly this structure:

                {
                  "intent": "StructuredCalculation",
                  "operation": "TopPayer"
                }
                """);

            chatHistory.AddUserMessage(question);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel, cancellationToken: cancellationToken);

            var content = response.Content ?? string.Empty;

            IntentAnalysisResponse? result;

            try
            {
                result = JsonSerializer.Deserialize<IntentAnalysisResponse>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (JsonException)
            {
                return CreateFallbackAnalysis();
            }

            if (result is null)
            {
                return CreateFallbackAnalysis();
            }

            var hasValidIntent = Enum.TryParse<AiQueryIntent>(result.Intent, ignoreCase: true, out var intent);

            var hasValidOperation = Enum.TryParse<AiQueryOperation>(result.Operation, ignoreCase: true, out var operation);

            if (!hasValidIntent)
            {
                return CreateFallbackAnalysis();
            }

            if (!hasValidOperation)
            {
                operation = AiQueryOperation.None;
            }

            if (intent != AiQueryIntent.StructuredCalculation)
            {
                operation = AiQueryOperation.None;
            }

            return new AiQueryAnalysis(intent, operation);
        }

        public async Task<AiQueryIntent> DetermineIntentAsync(string question, CancellationToken cancellationToken)
        {
            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage(
                """
                You classify questions for Circlo, a group expense application.

                There are only two intents:

                Semantic:
                Use when the user wants descriptions, summaries,
                explanations, recommendations, or general information
                about expenses.

                Calculation:
                Use when the user asks for totals, amounts, counts,
                averages, comparisons, highest/lowest spending,
                differences, or any exact numeric calculation.

                Return ONLY valid JSON in this exact format:

                {
                  "intent": "Semantic"
                }

                or

                {
                  "intent": "Calculation"
                }
                """
            );  

            chatHistory.AddUserMessage(question);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel, cancellationToken: cancellationToken);

            var content = response.Content ?? string.Empty;

            var result = JsonSerializer.Deserialize<IntentResponse>(content,
                                                        new JsonSerializerOptions
                                                        {
                                                            PropertyNameCaseInsensitive = true
                                                        });

            if (result is null || !Enum.TryParse<AiQueryIntent>(result.Intent, true, out var intent))
            {
                return AiQueryIntent.Semantic; 
            }

            return intent;
        }

        private class IntentResponse
        {
            public string Intent { get; set; } = string.Empty;
        }

        private static AiQueryAnalysis CreateFallbackAnalysis()
        {
            return new AiQueryAnalysis(AiQueryIntent.General, AiQueryOperation.None);
        }

        private sealed class IntentAnalysisResponse
        {
            public string Intent { get; set; } = string.Empty;

            public string Operation { get; set; } = string.Empty;
        }
    }
}
