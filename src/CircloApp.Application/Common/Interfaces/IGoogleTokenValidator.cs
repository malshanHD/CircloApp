namespace CircloApp.Application.Common.Interfaces
{
    public interface IGoogleTokenValidator
    {
        Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
    }
}
