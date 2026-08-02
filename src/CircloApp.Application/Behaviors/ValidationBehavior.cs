using FluentValidation;
using MediatR;

namespace CircloApp.Application.Behaviors
{
    public class ValidationBehavior<TReqest, TResponse> : IPipelineBehavior<TReqest, TResponse>
        where TReqest : notnull
    {
        private readonly IEnumerable<IValidator<TReqest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TReqest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TReqest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TReqest>(request);

                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Any())
                {
                    throw new ValidationException(failures);
                }
            }
            return await next();
        }
    }
}
