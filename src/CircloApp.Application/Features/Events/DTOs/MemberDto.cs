using CircloApp.Domain.Enums;

namespace CircloApp.Application.Features.Events.DTOs
{
    public class MemberDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
