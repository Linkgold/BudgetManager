using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UI.Services;

namespace UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            string apiUrl = builder.Configuration["ApiUrl"] ?? builder.HostEnvironment.BaseAddress;

            Console.WriteLine($"🌍 Entorno: {builder.HostEnvironment.Environment}");
            Console.WriteLine($"🔗 API URL: {apiUrl}");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

            // 🔥 Registrar AuthService
            builder.Services.AddScoped<IAuthService, AuthService>();

            await builder.Build().RunAsync();
        }
    }
}
