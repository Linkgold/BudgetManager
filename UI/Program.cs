using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UI.Configuration.Extensions;

namespace UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            ConfigureRootComponents(builder);

            builder.Services
                   .AddConfigurationServices(builder.Configuration)
                   .AddHttpClientServices()
                   .AddApplicationServices()
                   .AddAuthenticationServices();

            await builder.Build().RunAsync();
        }

        private static void ConfigureRootComponents(WebAssemblyHostBuilder builder)
        {
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
        }
    }
}