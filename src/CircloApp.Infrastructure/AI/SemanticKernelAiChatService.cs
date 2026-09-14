using CircloApp.Application.Interfaces.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CircloApp.Infrastructure.AI
{
    public class SemanticKernelAiChatService : IAiChatService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;

        public SemanticKernelAiChatService(IOptions<AzureAIOptionsII> azureAIOptions)
        {
            var aiOptions = azureAIOptions.Value;
            var builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(deploymentName: aiOptions.ChatDeploymentName, endpoint: aiOptions.Endpoint, apiKey: aiOptions.Apikey);
            _kernel = builder.Build();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string> AskAsync(string question, CancellationToken cancellationToken = default)
        {
            var chatHistory = new ChatHistory();

            chatHistory.AddSystemMessage(
                                        """
                                        You are Circlo AI assistant for a group of expense management application
                                        
                                        Answer question clearly and concisely.
                                        """);

            chatHistory.AddUserMessage(question);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: _kernel, cancellationToken: cancellationToken);

            return response.Content ?? string.Empty;
        }
    }
}
