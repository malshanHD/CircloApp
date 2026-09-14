using CircloApp.Application.Features.AI_II.Models;
using CircloApp.Application.Interfaces.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace CircloApp.Infrastructure.AI
{
    public class SemanticKernelAiIntentService : IAiIntentService
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
    }
}
