namespace CircloApp.Domain.Entities
{
    public class EventAiAnalysis
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string DataHash { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public BudgetEvent Event { get; set; } = null!;
    }
}
