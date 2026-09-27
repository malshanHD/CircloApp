namespace CircloApp.Application.Common.Interfaces
{
    public sealed record GoogleUserInfo(
        string GoogleSubject,
        string Email,
        bool IsAuthoritativeEmail,
        string? GivenName,
        string? FamilyName,
        string? Name,
        string? PictureUrl);
}
