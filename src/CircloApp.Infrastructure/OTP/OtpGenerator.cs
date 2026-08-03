using CircloApp.Application.Interfaces;
using System.Security.Cryptography;

namespace CircloApp.Infrastructure.OTP
{
    public class OtpGenerator : IOtpGenerator
    {
        public string GenerateOtp(int length)
        {
            return RandomNumberGenerator
            .GetInt32(100000, 999999)
            .ToString();
        }
    }
}
