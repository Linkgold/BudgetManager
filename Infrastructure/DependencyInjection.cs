using Domain.Interfaces.Repositories;
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    /// <summary>
    /// Configuración de inyección de dependencias para Infrastructure
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra todos los servicios de Infrastructure
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, DatabaseType databaseType = DatabaseType.SQLite)
        {
            // Registrar la fábrica apropiada
            services.AddSingleton<IDbContextFactory>
            (
                provider =>
                {
                    return CreateFactory(configuration, databaseType);
                }
            );

            // Registrar DbContext
            services.AddScoped<ApplicationDbContext>
            (
                provider =>
                {
                    IDbContextFactory factory = provider.GetRequiredService<IDbContextFactory>();
                    return factory.CreateDbContext();
                }
            );

            // Registrar repositorios
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            return services;
        }

        private static IDbContextFactory CreateFactory(IConfiguration configuration, DatabaseType databaseType)
        {
            string connectionString = GetConnectionString(configuration, databaseType);

            return databaseType switch
            {
                DatabaseType.SQLite => new SqliteDbContextFactory(connectionString),
                // DatabaseType.SqlServer => new SqlServerDbContextFactory(connectionString),
                // DatabaseType.PostgreSQL => new PostgreSQLDbContextFactory(connectionString),
                // DatabaseType.MySQL => new MySQLDbContextFactory(connectionString),
                _ => throw new NotSupportedException($"Database type '{databaseType}' is not supported")
            };
        }

        private static string GetConnectionString(IConfiguration configuration, DatabaseType databaseType)
        {
            string key = "DefaultConnection";

            string? connectionString = configuration.GetConnectionString(key);

            if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException($"Connection string '{key}' not found in configuration");

            return connectionString;
        }
    }

    /// <summary>
    /// Enumeración de tipos de base de datos soportados
    /// </summary>
    public enum DatabaseType
    {
        SQLite,
        // SqlServer,
        // PostgreSQL,
        // MySQL
    }
}