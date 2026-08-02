using FluentValidation;

namespace CircloApp.Application.Features.Authentication.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Request.UsernameOrEmail).NotEmpty();
            RuleFor(x => x.Request.Password).NotEmpty();
        }
    }
}
