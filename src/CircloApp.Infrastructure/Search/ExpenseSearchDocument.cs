using System.Text.Json.Serialization;

namespace CircloApp.Infrastructure.Search
{
    public class ExpenseSearchDocument
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("eventId")]
        public string EventId { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("embedding")]
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
