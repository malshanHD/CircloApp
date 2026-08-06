using FluentValidation;

namespace CircloApp.Application.Features.Events.Queries.GetEventDetails
{
    public class GetEventDetailsQueryValidator : AbstractValidator<GetEventDetailsQuery>
    {
        public GetEventDetailsQueryValidator()
        {
            RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event Id is required.");
        }
    }
}
