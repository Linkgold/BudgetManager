using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace API
{
    /// <summary>
    /// Punto de entrada principal de la aplicación
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Método Main - Entry point de la aplicación
        /// </summary>
        /// <param name="args">Argumentos de línea de comandos</param>
        public static void Main(string[] args)
        {
            IHostBuilder hostBuilder = CreateHostBuilder(args);
            IHost host = hostBuilder.Build();
            host.Run();
        }

        /// <summary>
        /// Crea y configura el HostBuilder de la aplicación
        /// </summary>
        /// <param name="args">Argumentos de línea de comandos</param>
        /// <returns>IHostBuilder configurado</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults
                (
                    webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    }
                );

            return hostBuilder;
        }
    }
}