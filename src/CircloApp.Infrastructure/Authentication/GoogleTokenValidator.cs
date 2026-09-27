using CircloApp.Application.Common.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CircloApp.Infrastructure.Authentication
{
    internal sealed class GoogleTokenValidator(IOptions<GoogleAuthOptions> options) : IGoogleTokenValidator
    {
        private readonly string _clientId = options.Value.ClientId;

        public async Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(idToken))
            {
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_clientId]
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                if (!payload.EmailVerified || 
                    string.IsNullOrWhiteSpace(payload.Subject) || 
                    string.IsNullOrWhiteSpace(payload.Email))
                {
                    return null;
                }

                var isAuthoritativeEmail =
                    payload.Email.EndsWith(
                        "@gmail.com",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    (payload.EmailVerified &&
                     !string.IsNullOrWhiteSpace(payload.HostedDomain));

                return new GoogleUserInfo(
                    GoogleSubject: payload.Subject,
                    Email: payload.Email.Trim().ToLowerInvariant(),
                    IsAuthoritativeEmail: isAuthoritativeEmail,
                    GivenName: payload.GivenName,
                    FamilyName: payload.FamilyName,
                    Name: payload.Name,
                    PictureUrl: payload.Picture);

            }
            catch (InvalidJwtException)
            {
                return null;
            }
        }
    }
}
