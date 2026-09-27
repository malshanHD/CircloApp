using CircloApp.Domain.Entities;

namespace CircloApp.Application.Interfaces
{
    public sealed record GeneratedAccessToken(string AccessToken, DateTime ExpiresAt);

    public interface IJwtTokenGenerator
    {
        GeneratedAccessToken GenerateToken(User user);
    }
}
