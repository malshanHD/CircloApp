using CircloApp.Application.Interfaces.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace CircloApp.Infrastructure.AI
{
    public class SemanticKernelEmbeddingService : IEmbeddingServiceII
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        public SemanticKernelEmbeddingService(IOptions<AzureAIOptionsII> azureAIOptions)
        {
            var aiOptions = azureAIOptions.Value;
            var builder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0001, SKEXP0010

            builder.AddAzureOpenAIEmbeddingGenerator(
                deploymentName: aiOptions.EmbeddingDeploymentName,
                endpoint: aiOptions.Endpoint,
                apiKey: aiOptions.Apikey);

#pragma warning restore SKEXP0001, SKEXP0010

            var kernel = builder.Build();

            _embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        }

        public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
        {
            var embedding = await _embeddingGenerator.GenerateAsync(text, cancellationToken: cancellationToken);

            return embedding.Vector;
        }
    }
}
