namespace CircloApp.Application.Features.Authentication.DTOs
{
    public class RegisterResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public bool RequiresOtpVerification { get; set; }
    }
}
