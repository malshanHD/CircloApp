using CircloApp.Application.Features.Authentication.Commands;
using FluentValidation;

namespace CircloApp.Application.Validators
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Request.FirstName)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(x => x.Request.LastName)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(x => x.Request.Email)
                .NotEmpty()
                .EmailAddress();
            RuleFor(x => x.Request.Username)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(x => x.Request.Password)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
}
