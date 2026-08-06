using CircloApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CircloApp.Infrastructure.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContext)
        {
            _contextAccessor = httpContext;
        }
        public Guid UserId
        {
            get
            {
                var value = _contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(value))
                    throw new UnauthorizedAccessException();

                return Guid.Parse(value);
            }
        }
    }
}
