using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Factories
{
    /// <summary>
    /// Fábrica para crear DbContext con SQLite
    /// </summary>
    public class SqliteDbContextFactory : IDbContextFactory
    {
        private readonly string _connectionString;
        private DbContextOptions<ApplicationDbContext>? _options;

        public SqliteDbContextFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

            _connectionString = connectionString;
            _options = null;

            EnsureDirectoryExists(_connectionString);
        }

        public ApplicationDbContext CreateDbContext()
        {
            DbContextOptions<ApplicationDbContext> options = GetOptions();
            return new ApplicationDbContext(options);
        }

        public DbContextOptions<ApplicationDbContext> GetOptions()
        {
            if (_options != null) return _options;

            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(_connectionString)
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };
            DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Configurar SQLite
            optionsBuilder.UseSqlite
            (
                builder.ConnectionString,
                sqliteOptions =>
                {
                    // Configuraciones específicas de SQLite
                    sqliteOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }
            );

            // Configuraciones adicionales comunes
            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.EnableDetailedErrors(false);

            _options = optionsBuilder.Options;
            return _options;
        }

        private void EnsureDirectoryExists(string connectionString)
        {
            SqliteConnectionStringBuilder builder = new(connectionString);

            string? directory = Path.GetDirectoryName(builder.DataSource);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}