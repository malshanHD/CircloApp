using FluentValidation;

namespace CircloApp.Application.Features.Events.Queries.GetMyEvents
{
    public class GetMyEventsQueryValidator : AbstractValidator<GetMyEventsQuery>
    {
        public GetMyEventsQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
