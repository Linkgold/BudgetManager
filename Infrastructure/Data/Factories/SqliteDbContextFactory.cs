using Infrastructure.Data.Factories.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Factories
{
    /// <summary>
    /// Fábrica para crear DbContext con SQLite
    /// </summary>
    public class SqliteDbContextFactory : IDbContextFactory, IDatabaseBackup
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

        // ========== Implementación de IDbContextFactory ==========

        public string GetConnectionString() => _connectionString;

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

        // ========== Implementación de IDatabaseBackup ==========

        public Task BackupAsync(string destinationPath)
        {
            if (string.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Destination path cannot be empty", nameof(destinationPath));

            // Obtener la ruta del archivo desde la connection string
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder(_connectionString);
            string sourcePath = builder.DataSource;

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"SQLite database file not found at {sourcePath}");

            // Crear directorio de destino si no existe
            string? destinationDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDir))
                Directory.CreateDirectory(destinationDir);

            // Copiar el archivo (sobrescribe si existe)
            File.Copy(sourcePath, destinationPath, overwrite: true);

            return Task.CompletedTask;
        }

        // ========== Método privado auxiliar ==========

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