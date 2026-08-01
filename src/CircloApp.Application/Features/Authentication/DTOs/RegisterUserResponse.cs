namespace CircloApp.Application.Features.Authentication.DTOs
{
    public class RegisterUserResponse
    {
        public Guid UserId { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
