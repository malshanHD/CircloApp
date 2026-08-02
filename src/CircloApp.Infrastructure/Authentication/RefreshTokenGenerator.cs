using CircloApp.Application.Interfaces;
using System.Security.Cryptography;

namespace CircloApp.Infrastructure.Authentication
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string Generate()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
