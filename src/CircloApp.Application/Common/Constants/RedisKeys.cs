namespace CircloApp.Application.Common.Constants
{
    public static class RedisKeys
    {
        public static string Registration(string email) => $"registration:{email.ToLower()}";
        public static string ForgotPassword(string email) => $"forgot-password:{email.ToLower()}";
    }
}
