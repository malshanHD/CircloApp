using CircloApp.Application.Exceptions;
using CircloApp.Shared.Responses;
using FluentValidation;


namespace CircloApp.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BadRequestException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(
                ApiResponse<object>.FailureResponse(ex.Message));
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();

                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.FailureResponse("Validation failed.", errors));
            }
            catch (Exception)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(
                ApiResponse<object>.FailureResponse("An unexpected error occurred."));
            }
        }
    }
}
