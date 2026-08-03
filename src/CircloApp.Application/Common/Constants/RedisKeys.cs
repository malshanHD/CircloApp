using System;
using System.Collections.Generic;
using System.Text;

namespace CircloApp.Application.Common.Constants
{
    public static class RedisKeys
    {
        public static string Registration(string email) => $"registration:{email.ToLower()}";
        public static string ForgotPassword(string email) => $"forgot-password:{email.ToLower()}";
    }
}
