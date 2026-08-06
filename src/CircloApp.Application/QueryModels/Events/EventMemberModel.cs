using CircloApp.Domain.Enums;

namespace CircloApp.Application.QueryModels.Events
{
    public class EventMemberModel
    {
        public Guid UserId { get; init; }

        public string Username { get; init; } = string.Empty;

        public string FullName { get; init; } = string.Empty;

        public string Role { get; init; } = string.Empty;
    }
}
