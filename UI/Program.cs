using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UI.Services;
using UI.Services.Interfaces;

namespace UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // 🔥 Configurar logging
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Services.AddScoped<ILogService, LogService>();

            string apiUrl = builder.Configuration["ApiUrl"] ?? builder.HostEnvironment.BaseAddress;

            Console.WriteLine($"🌍 Entorno: {builder.HostEnvironment.Environment}");
            Console.WriteLine($"🔗 API URL: {apiUrl}");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

            // 🔥 Registrar AuthService
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IStorageService, StorageService>();

            // 🔥 Registrar servicios de autenticación
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();  // ← REGISTRAR PRIMERO
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
            provider.GetRequiredService<CustomAuthenticationStateProvider>()); // ← REGISTRAR COMO AuthenticationStateProvider
            builder.Services.AddScoped<IAuthService, AuthService>(); // ← AHORA DEPENDE DE CustomAuthenticationStateProvider

            builder.Services.AddScoped<IThemeService, ThemeService>();

            await builder.Build().RunAsync();
        }
    }
}