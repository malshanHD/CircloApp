namespace CircloApp.Application.Features.Events.DTOs
{
    public class CreateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
