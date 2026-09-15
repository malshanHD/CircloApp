namespace CircloApp.Application.Features.Events.DTOs
{
    public class EventDetailsDto
    {
        public bool IsAdmin { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MemberDto> Members { get; set; } = [];
    }
}
