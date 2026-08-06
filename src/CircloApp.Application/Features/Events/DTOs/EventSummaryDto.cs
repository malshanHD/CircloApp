namespace CircloApp.Application.Features.Events.DTOs
{
    public class EventSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public bool IsAdmin { get; set; }
    }
}
