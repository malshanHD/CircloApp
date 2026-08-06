namespace CircloApp.Application.QueryModels.Events
{
    public class EventSummaryModel
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public DateTime CreatedAt { get; init; }

        public IReadOnlyCollection<EventMemberModel> Members { get; init; }
            = [];
    }
}
