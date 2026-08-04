using UI.Configuration;
using UI.Services.Interfaces;

namespace UI.Services
{
    public sealed class ConfigurationService : IConfigurationService
    {
        public ApiConfiguration Api { get; }

        public ConfigurationService(IConfiguration configuration)
        {
            string apiUrl = configuration["ApiUrl"] ?? throw new InvalidOperationException("ApiUrl is not configured.");

            Api = new ApiConfiguration { BaseUri = new Uri(apiUrl) };
        }
    }
}