using CircloApp.Application.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;
using System.ClientModel;

namespace CircloApp.Infrastructure.Services
{
    public class AzureEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;
        public AzureEmbeddingService(IOptions<AzureAIOptions> options)
        {
            var azureOption = options.Value;

            var openiAiClient = new OpenAIClient(
                new ApiKeyCredential(azureOption.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(azureOption.EmbeddingEndpoint)
                });

            _embeddingClient = openiAiClient.GetEmbeddingClient(azureOption.EmbeddingDeploymentName);
        }

        public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAync(string text, CancellationToken cancellationToken = default)
        {
            var response = await _embeddingClient.GenerateEmbeddingAsync(text, options: null, cancellationToken);

            return response.Value.ToFloats();
        }
    }
}
