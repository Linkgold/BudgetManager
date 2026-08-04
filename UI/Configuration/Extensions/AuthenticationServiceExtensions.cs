using Microsoft.AspNetCore.Components.Authorization;
using UI.Services;
using UI.Services.Interfaces;

namespace UI.Configuration.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            services.AddAuthorizationCore();

            services.AddScoped<CustomAuthenticationStateProvider>();

            services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthenticationStateProvider>());

            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}