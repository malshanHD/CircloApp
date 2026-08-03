using FluentValidation;

namespace CircloApp.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpValidator : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpValidator()
        {
            RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Request.Otp).NotEmpty().Length(6);
        }
    }
}
