using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
