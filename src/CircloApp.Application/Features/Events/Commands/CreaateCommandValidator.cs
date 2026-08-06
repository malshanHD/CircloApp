using FluentValidation;

namespace CircloApp.Application.Features.Events.Commands
{
    public class CreaateCommandValidator : AbstractValidator<CreateEventCommand>
    {
        public CreaateCommandValidator()
        {
            RuleFor(x => x.CreateEventRequest.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CreateEventRequest.Description).NotEmpty().MaximumLength(500);
        }
    }
}
