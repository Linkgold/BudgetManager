using Application.Interfaces;

namespace Application.Services
{
    public class JwtSettings : IJwtSettings
    {
        public string Key { get; }
        public string Issuer { get; }
        public string Audience { get; }
        public int ExpirationInMinutes { get; }

        public JwtSettings(string key, string issuer, string audience, int expirationInMinutes)
        {
            Key = key;
            Issuer = issuer;
            Audience = audience;
            ExpirationInMinutes = expirationInMinutes;
        }
    }
}