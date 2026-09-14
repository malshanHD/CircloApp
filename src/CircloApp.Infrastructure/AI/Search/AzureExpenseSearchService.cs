using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using CircloApp.Application.Interfaces.AI;
using CircloApp.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Management;

namespace CircloApp.Infrastructure.AI.Search
{
    public class AzureExpenseSearchService : IExpenseSearchService
    {
        private readonly SearchClient _searchClient;
        private readonly IEmbeddingServiceII _embeddingService;

        public AzureExpenseSearchService(IOptions<AzureSearchOptions> options, IEmbeddingServiceII embeddingService)
        {
            _embeddingService = embeddingService;
            var searchOptions = options.Value;
            _searchClient = new SearchClient(new Uri(searchOptions.Endpoint), searchOptions.IndexName, new Azure.AzureKeyCredential(searchOptions.ApiKey));
        }

        public async Task<IReadOnlyList<ExpenseSearchResult>> SearchExpensesAsync(Guid eventId, string query, CancellationToken cancellationToken)
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

            var vectorQuery = new VectorizedQuery(queryEmbedding)
            {
                KNearestNeighborsCount = 20,
            };

            vectorQuery.Fields.Add("embedding");

            var searchOptions = new SearchOptions
            {
                Size = 5,
                Filter = $"eventId eq '{eventId}'",
            };

            searchOptions.VectorSearch = new VectorSearchOptions();

            searchOptions.VectorSearch.Queries.Add(vectorQuery);

            var response = await _searchClient.SearchAsync<ExpenseSearchDocument_II>(searchText: null, searchOptions, cancellationToken);

            var results = new List<ExpenseSearchResult>();

            await foreach (var result in response.Value.GetResultsAsync())
            {
                if (!Guid.TryParse(result.Document.Id, out var expenseId))
                {
                    continue;
                }

                results.Add(
                    new ExpenseSearchResult
                    (
                        expenseId,
                        result.Document.Description,
                        (decimal)result.Document.Amount,
                        result.Score ?? 0
                     )
                );
            }

            return results;
        }
    }
}
