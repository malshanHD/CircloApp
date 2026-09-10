using Azure.Search.Documents;
using CircloApp.Application.Interfaces.AI;
using CircloApp.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CircloApp.Infrastructure.AI.Search
{
    public class AzureExpenseSearchIndexer : IExpenseSearchIndexer
    {
        private readonly SearchClient _searchClient;
        private readonly IEmbeddingServiceII _embeddingService;

        public AzureExpenseSearchIndexer(IOptions<AzureSearchOptions> options, IEmbeddingServiceII embeddingService)
        {
            var searchOptions = options.Value;
            _embeddingService = embeddingService;

            _searchClient = new SearchClient(new Uri(searchOptions.Endpoint), searchOptions.IndexName, new Azure.AzureKeyCredential(searchOptions.ApiKey));
        }

        public async Task IndexExpenseAsync(Guid expenseId, Guid eventId, string description, decimal amount, CancellationToken cancellationToken)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(description, cancellationToken);

            var document = new ExpenseSearchDocument_II
            {
                Id = expenseId.ToString(),
                EventId = eventId.ToString(),
                Description = description,
                Amount = (double)amount,
                Embedding = embedding
            };

            await _searchClient.MergeOrUploadDocumentsAsync(new[] { document }, cancellationToken: cancellationToken);
        }
    }
}
