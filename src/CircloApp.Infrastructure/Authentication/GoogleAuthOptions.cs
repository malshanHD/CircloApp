namespace CircloApp.Infrastructure.Authentication
{
    public sealed class GoogleAuthOptions
    {
        public const string SectionName = "Authentication:Google";
        public required string ClientId { get; init; }
    }
}
