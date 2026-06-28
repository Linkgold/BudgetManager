using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Extensiones para configurar servicios de Infrastructure
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra los servicios de Infrastructure en el contenedor DI
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, string databaseType = "SQLite")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // Registrar DbContext Factory según el tipo de base de datos
            IDbContextFactory dbFactory = CreateDbContextFactory(configuration, databaseType);

            // Registrar la fábrica como singleton
            services.AddSingleton<IDbContextFactory>(dbFactory);

            // Registrar DbContext usando la fábrica
            services.AddDbContext<ApplicationDbContext>
            (
                (serviceProvider, optionsBuilder) =>
                {
                    DbContextOptions<ApplicationDbContext> options = dbFactory.GetOptions();
                    // No podemos pasar options directamente, así que configuramos con la fábrica
                    // O mejor: registrar el DbContext directamente
                }
            );

            // Registrar repositorios
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            return services;
        }

        /// <summary>
        /// Crea la fábrica de DbContext según el tipo de base de datos
        /// </summary>
        private static IDbContextFactory CreateDbContextFactory(IConfiguration configuration, string databaseType)
        {
            string connectionString;

            switch (databaseType.ToLower())
            {
                case "sqlite":
                    connectionString = configuration.GetConnectionString("SQLiteConnection");
                    if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("SQLite connection string not found in configuration");

                    return new SqliteDbContextFactory(connectionString);

                // Preparado para futuras bases de datos
                // case "sqlserver":
                //     connectionString = configuration.GetConnectionString("SqlServerConnection");
                //     return new SqlServerDbContextFactory(connectionString);
                // 
                // case "postgresql":
                //     connectionString = configuration.GetConnectionString("PostgreSqlConnection");
                //     return new PostgreSqlDbContextFactory(connectionString);
                // 
                // case "mysql":
                //     connectionString = configuration.GetConnectionString("MySqlConnection");
                //     return new MySqlDbContextFactory(connectionString);

                default:
                    throw new NotSupportedException($"Database type '{databaseType}' is not supported");
            }
        }
    }
}