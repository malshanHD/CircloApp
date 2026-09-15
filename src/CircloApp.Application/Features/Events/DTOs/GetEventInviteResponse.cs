namespace CircloApp.Application.Features.Events.DTOs
{
    public class GetEventInviteResponse
    {
        public int InvitationsCount { get; set; }
        public List<GetEventInviteResponseList> InviteDetails { get; set; }
    }

    public class GetEventInviteResponseList
    {
        public Guid EventId { get; set; }
        public string InviterName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
    }
}
