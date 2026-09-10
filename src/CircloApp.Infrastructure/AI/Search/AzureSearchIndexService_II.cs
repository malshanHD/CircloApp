using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using CircloApp.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CircloApp.Infrastructure.AI.Search
{
    public class AzureSearchIndexService_II
    {
        private readonly SearchIndexClient _client;
        private readonly string _indexName;

        public AzureSearchIndexService_II(IOptions<AzureSearchOptions> options)
        {
            var searchOptions = options.Value;
            _indexName = searchOptions.IndexName;

            _client = new SearchIndexClient(new Uri(searchOptions.Endpoint), new AzureKeyCredential(searchOptions.ApiKey));
        }

        public async Task CreateIndexIfNotExistsAsync(CancellationToken cancellationToken = default)
        {
            var fields = new List<SearchField>
            {
                new SimpleField("id", SearchFieldDataType.String)
                {
                    IsKey = true,
                    IsFilterable = true
                },

                new SimpleField("eventId", SearchFieldDataType.String)
                {
                    IsFilterable = true
                },

                new SearchableField("description")
                {
                    IsFilterable = false,
                },

                new SimpleField("amount", SearchFieldDataType.Double)
                {
                    IsFilterable = true,
                    IsSortable = true
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

            var index = new SearchIndex(_indexName, fields)
            {
                VectorSearch = vectorSearch
            };

            await _client.CreateOrUpdateIndexAsync(index, cancellationToken: cancellationToken);
        }
    }
}
