using Infrastructure.Data.Backup;
using Infrastructure.Data.Factories;
using Infrastructure.Data.Factories.Interfaces;
using Microsoft.Data.Sqlite;
using Tests.Helpers;

namespace Tests.Infrastructure.Backup
{
    public class DatabaseBackupIntegrationTests : IDisposable
    {
        private readonly string _tempDbDir;
        private readonly string _tempBackupDir;
        private readonly string _dbPath;
        private readonly IDatabaseBackup _backup; // <- Solo la interfaz

        public DatabaseBackupIntegrationTests()
        {
            _tempDbDir = TestBackupHelper.CreateTempDirectory("DbTest");
            _tempBackupDir = TestBackupHelper.CreateTempDirectory("BackupTest");

            _dbPath = Path.Combine(_tempDbDir, "test.db");

            using (SqliteConnection connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();
                connection.Close();
            }

            string connectionString = $"Data Source={_dbPath}";
            SqliteDbContextFactory factory = new SqliteDbContextFactory(connectionString);
            _backup = factory; // <- Asignación directa
        }

        public void Dispose()
        {
            TestBackupHelper.CleanupTempDirectory(_tempDbDir);
            TestBackupHelper.CleanupTempDirectory(_tempBackupDir);
        }

        [Fact]
        public async Task BackupAsync_ShouldCopyDatabaseFile()
        {
            string destinationPath = Path.Combine(_tempBackupDir, "backup_test.db");
            await _backup.BackupAsync(destinationPath);

            Assert.True(File.Exists(destinationPath), $"El archivo destino {destinationPath} no se creó");
        }

        [Fact]
        public async Task BackupAsync_WhenDestinationDirectoryDoesNotExist_ShouldCreateIt()
        {
            string nestedDir = Path.Combine(_tempBackupDir, "Nested", "Directory");
            string destinationPath = Path.Combine(nestedDir, "backup_test.db");

            await _backup.BackupAsync(destinationPath);

            Assert.True(Directory.Exists(nestedDir));
            Assert.True(File.Exists(destinationPath));
        }

        [Fact]
        public async Task BackupAsync_WhenDestinationFileExists_ShouldOverwrite()
        {
            string destinationPath = Path.Combine(_tempBackupDir, "backup_test.db");
            File.WriteAllText(destinationPath, "contenido antiguo");

            await _backup.BackupAsync(destinationPath);

            string content = File.ReadAllText(destinationPath);
            Assert.NotEqual("contenido antiguo", content);
        }

        [Fact]
        public async Task BackupAsync_WhenSourceDatabaseDoesNotExist_ShouldThrowFileNotFoundException()
        {
            string nonExistentDbPath = Path.Combine(_tempDbDir, "nonexistent.db");
            string connectionString = $"Data Source={nonExistentDbPath}";
            SqliteDbContextFactory factoryWithMissingDb = new SqliteDbContextFactory(connectionString);
            IDatabaseBackup backupWithMissingDb = factoryWithMissingDb;

            string destinationPath = Path.Combine(_tempBackupDir, "backup_test.db");

            FileNotFoundException exception = await Assert.ThrowsAsync<FileNotFoundException>(
                () => backupWithMissingDb.BackupAsync(destinationPath));

            Assert.Contains("SQLite database file not found", exception.Message);
        }

        [Fact]
        public void GetBackupBaseDirectory_WithOrchestrator_ShouldReturnCorrectPath()
        {
            // Necesitamos la factory para el orquestador, pero no la tenemos como campo
            // Recreamos la factory para esta prueba
            string connectionString = $"Data Source={_dbPath}";
            SqliteDbContextFactory factory = new SqliteDbContextFactory(connectionString);
            BackupOrchestrator orchestrator = TestBackupHelper.CreateRealOrchestrator(factory);

            string expectedBackupDir = Path.Combine(_tempDbDir, "Backup");
            string result = orchestrator.GetBackupBaseDirectory();

            Assert.Equal(expectedBackupDir, result);
        }

        [Fact]
        public async Task Orchestrator_ShouldExecuteBackupAndCreateFile()
        {
            BackupSettings settings = TestBackupHelper.CreateSettings(backupDirectory: _tempBackupDir);
            SqliteDbContextFactory factory = new SqliteDbContextFactory($"Data Source={_dbPath}");
            BackupOrchestrator orchestrator = TestBackupHelper.CreateRealOrchestrator(factory, settings);

            await orchestrator.PerformBackupAsync(CancellationToken.None);

            string[] backupFiles = Directory.GetFiles(_tempBackupDir, "backup_*.db");
            Assert.Single(backupFiles);
        }
    }
}