namespace CircloApp.Application.Features.Events.DTOs
{
    public class CreateEventRespose
    {
        public Guid EventId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
