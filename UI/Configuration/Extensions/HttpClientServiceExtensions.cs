using UI.Services.Interfaces;

namespace UI.Configuration.Extensions
{
    public static class HttpClientServiceExtensions
    {
        public static IServiceCollection AddHttpClientServices(this IServiceCollection services)
        {
            services.AddScoped
            (
                provider =>
                {
                    IConfigurationService configuration = provider.GetRequiredService<IConfigurationService>();

                    return new HttpClient { BaseAddress = configuration.Api.BaseUri };
                }
            );

            return services;
        }
    }
}