using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using CircloApp.Application.Features.AI.DTO;
using CircloApp.Application.Interfaces;
using CircloApp.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CircloApp.Infrastructure.Search
{
    public class AzureExpenseVectorSearchService : IExpenseVectorSearchService
    {
        private readonly SearchIndexClient _indexClient;
        private readonly AzureSearchOptions _searchOptions;
        private readonly SearchClient _searchClient;
        private readonly IEmbeddingService _embeddingService;

        public AzureExpenseVectorSearchService(IOptions<AzureSearchOptions> options, IEmbeddingService embeddingService)
        {
            _searchOptions = options.Value;
            _embeddingService = embeddingService;

            var credential = new AzureKeyCredential(_searchOptions.ApiKey);

            _indexClient = new SearchIndexClient(new Uri(_searchOptions.Endpoint), credential);

            _searchClient = new SearchClient(new Uri(_searchOptions.Endpoint), _searchOptions.IndexName, credential);
        }

        public async Task CreateIndexAsync(CancellationToken cancellationToken = default)
        {
            var fields = new List<SearchField>
            {
                new SearchField("id", SearchFieldDataType.String)
                {
                    IsKey = true,
                    IsFilterable = true,
                },

                new SearchField("eventId", SearchFieldDataType.String)
                {
                    IsFilterable = true
                },

                new SearchField("description", SearchFieldDataType.String)
                {
                    IsSearchable = true
                },

                new SearchField("amount", SearchFieldDataType.Double)
                {
                    IsFilterable = true,
                    IsSortable = true,
                },

                new SearchField("embedding", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    VectorSearchDimensions = 1536,
                    VectorSearchProfileName = "circlo-vector-profile"
                }
            };

            var vectorSearch = new VectorSearch
            {
                Algorithms =
                {
                    new HnswAlgorithmConfiguration("circlo-hnsw")
                    {
                        Parameters = new HnswParameters
                        {
                            Metric = VectorSearchAlgorithmMetric.Cosine
                        }
                    }
                },

                Profiles =
                {
                    new VectorSearchProfile("circlo-vector-profile", "circlo-hnsw")
                }
            };

            var index = new SearchIndex(_searchOptions.IndexName, fields)
            {
                VectorSearch = vectorSearch
            };

            await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: cancellationToken);
        }

        public async Task DeleteIndexAsync(CancellationToken cancellationToken = default)
        {
            await _indexClient.DeleteIndexAsync(_searchOptions.IndexName,cancellationToken);
        }

        public async Task<List<ExpenseVectorSearchResult>> SearchExpenseAsync(Guid eventId, string query, CancellationToken cancellationToken = default)
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAync(query, cancellationToken);

            var vectorQuery = new VectorizedQuery(queryEmbedding)
            {
                KNearestNeighborsCount = 5,
                Fields =
                {
                    "embedding"
                }
            };

            var searchOptions = new SearchOptions
            {
                Size = 5,
                Filter = $"eventId eq '{eventId}'"
            };

            searchOptions.VectorSearch = new VectorSearchOptions();
            searchOptions.VectorSearch.Queries.Add(vectorQuery);

            var response = await _searchClient.SearchAsync<ExpenseSearchDocument>(
                searchText: query,
                searchOptions,
                cancellationToken);

            var results = new List<ExpenseVectorSearchResult>();

            await foreach (var result in response.Value.GetResultsAsync())
            {
                results.Add(
                        new ExpenseVectorSearchResult
                        {
                            Id = result.Document.Id,
                            Description = result.Document.Description,
                            Amount = result.Document.Amount,
                            Score = result.Score ?? 0
                        });
            }

            return results;
        }

        public async Task UploadExpenseAsync(Guid expenseId, Guid eventId, string description, decimal amount, CancellationToken cancellationToken = default)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAync(description, cancellationToken);

            var document = new ExpenseSearchDocument
            {
                Id = expenseId.ToString(),
                EventId = eventId.ToString(),
                Description = description,
                Amount = (double)amount,
                Embedding = embedding
            };

            await _searchClient.UploadDocumentsAsync(new[] { document }, cancellationToken: cancellationToken);
        }
    }
}
