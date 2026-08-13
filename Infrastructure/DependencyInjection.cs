using Domain.Interfaces;
using Domain.Interfaces.Managers;
using Infrastructure.Data;
using Infrastructure.Data.Factories;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, DatabaseTypeEnum databaseType = DatabaseTypeEnum.SQLite)
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
            services.AddScoped<ITransactionManager, TransactionManager>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IFixedExpenseRepository, FixedExpenseRepository>();
            services.AddScoped<IBudgetRepository, BudgetRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            

            return services;
        }

        private static IDbContextFactory CreateFactory(IConfiguration configuration, DatabaseTypeEnum databaseType)
        {
            string connectionString = GetConnectionString(configuration, databaseType);

            return databaseType switch
            {
                DatabaseTypeEnum.SQLite => new SqliteDbContextFactory(connectionString),
                // DatabaseType.SqlServer => new SqlServerDbContextFactory(connectionString),
                // DatabaseType.PostgreSQL => new PostgreSQLDbContextFactory(connectionString),
                // DatabaseType.MySQL => new MySQLDbContextFactory(connectionString),
                _ => throw new NotSupportedException($"Database type '{databaseType}' is not supported")
            };
        }

        private static string GetConnectionString(IConfiguration configuration, DatabaseTypeEnum databaseType)
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
    public enum DatabaseTypeEnum
    {
        SQLite,
        // SqlServer,
        // PostgreSQL,
        // MySQL
    }
}