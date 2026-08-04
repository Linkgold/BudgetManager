using UI.Services;
using UI.Services.Interfaces;

namespace UI.Configuration.Extensions
{
    public static class ConfigurationServiceExtensions
    {
        public static IServiceCollection AddConfigurationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConfigurationService>(new ConfigurationService(configuration));

            return services;
        }
    }
}