using MudBlazor.Services;
using UI.Services;
using UI.Services.API;
using UI.Services.Interfaces;

namespace UI.Configuration.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IStorageService, StorageService>();

            services.AddScoped<IToastService, ToastService>();

            services.AddScoped<IThemeService, ThemeService>();

            services.AddScoped<ILogService, LogService>();

            services.AddScoped<APIService>();

            services.AddScoped<HasDataService>();

            services.AddMudServices();

            return services;
        }
    }
}