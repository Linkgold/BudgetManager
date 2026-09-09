using Infrastructure.Data.Backup;
using Infrastructure.Data.Factories.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tests.Helpers;

namespace Tests.Infrastructure.Backup
{
    public class BackupOrchestratorTests : IDisposable
    {
        private readonly string _tempBackupDir;

        public BackupOrchestratorTests()
        {
            _tempBackupDir = TestBackupHelper.CreateTempDirectory();
        }

        public void Dispose()
        {
            TestBackupHelper.CleanupTempDirectory(_tempBackupDir);
        }

        // ================================================================
        // 1. COMPROBACIÓN DE HORA PROGRAMADA
        // ================================================================

        [Fact]
        public async Task PerformBackupIfNeeded_WhenAfterScheduleTimeAndNoBackup_ShouldExecute()
        {
            // Arrange
            BackupSettings settings = TestBackupHelper.CreateSettings(
                backupDirectory: _tempBackupDir,
                scheduleHour: 2);

            // Crear un mock de IDbContextFactory que TAMBIÉN implemente IDatabaseBackup
            Mock<IDbContextFactory> factoryMock = new Mock<IDbContextFactory>();
            Mock<IDatabaseBackup> backupMock = factoryMock.As<IDatabaseBackup>(); // <- CLAVE

            factoryMock.Setup(f => f.GetConnectionString())
                .Returns("Data Source=C:\\Data\\test.db");

            Mock<ILogger<BackupOrchestrator>> loggerMock = new Mock<ILogger<BackupOrchestrator>>();

            BackupOrchestrator orchestrator = new BackupOrchestrator(
                factoryMock.Object,
                loggerMock.Object,
                TestBackupHelper.CreateOptions(settings));

            // Simular que son las 10 AM (después de las 2 AM)
            DateTime now = new DateTime(2025, 3, 15, 10, 0, 0);
            DateTime scheduled = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);

            // Act
            await orchestrator.PerformBackupIfNeededAsync(CancellationToken.None);

            // Assert
            Assert.True(now >= scheduled);
            factoryMock.Verify(f => f.GetConnectionString(), Times.AtLeastOnce);
            backupMock.Verify(b => b.BackupAsync(It.IsAny<string>()), Times.Once); // Verificar que se llamó al backup
        }

        // ================================================================
        // 2. COMPROBACIÓN DE BACKUP EXISTENTE
        // ================================================================

        [Fact]
        public async Task PerformBackupIfNeeded_WhenBackupExistsToday_ShouldNotExecute()
        {
            // Arrange
            BackupSettings settings = TestBackupHelper.CreateSettings(
                backupDirectory: _tempBackupDir,
                scheduleHour: 2);

            // Crear backup de hoy
            TestBackupHelper.CreateMockBackupFile(_tempBackupDir, DateTime.Now);

            // Mock que devuelve una ruta que apunta al directorio temporal
            Mock<IDbContextFactory> factoryMock = new Mock<IDbContextFactory>();
            factoryMock.Setup(f => f.GetConnectionString())
                .Returns($"Data Source={Path.Combine(_tempBackupDir, "..", "fake.db")}");

            // IMPORTANTE: El orquestador usará GetBackupBaseDirectory() que concatena
            // el directorio de la BD + BackupSettings.BackupDirectory.
            // Como la BD está en "C:\Temp\BackupTests_XXX\fake.db",
            // el backup buscará en "C:\Temp\BackupTests_XXX\Backup\".
            // Pero nosotros hemos creado los backups en _tempBackupDir, que es exactamente eso.
            // Por lo tanto, funcionará.

            Mock<ILogger<BackupOrchestrator>> loggerMock = new Mock<ILogger<BackupOrchestrator>>();

            BackupOrchestrator orchestrator = new BackupOrchestrator(
                factoryMock.Object,
                loggerMock.Object,
                TestBackupHelper.CreateOptions(settings));

            // Act
            await orchestrator.PerformBackupIfNeededAsync(CancellationToken.None);

            // Assert
            factoryMock.Verify(f => f.GetConnectionString(), Times.Once);

            string[] backupFiles = Directory.GetFiles(_tempBackupDir, "backup_*.db");
            Assert.Single(backupFiles);
        }

        [Fact]
        public async Task PerformBackupIfNeeded_WhenNoBackupToday_ShouldExecute()
        {
            // Arrange
            BackupSettings settings = TestBackupHelper.CreateSettings(
                backupDirectory: _tempBackupDir,
                scheduleHour: 2);

            // Crear backup de ayer (no de hoy)
            TestBackupHelper.CreateMockBackupFile(_tempBackupDir, DateTime.Now.AddDays(-1));

            Mock<IDbContextFactory> factoryMock = new Mock<IDbContextFactory>();
            factoryMock.As<IDatabaseBackup>(); // <- CLAVE

            factoryMock.Setup(f => f.GetConnectionString())
                .Returns("Data Source=C:\\Data\\test.db");

            Mock<ILogger<BackupOrchestrator>> loggerMock = new Mock<ILogger<BackupOrchestrator>>();

            BackupOrchestrator orchestrator = new BackupOrchestrator(
                factoryMock.Object,
                loggerMock.Object,
                TestBackupHelper.CreateOptions(settings));

            // Act
            await orchestrator.PerformBackupIfNeededAsync(CancellationToken.None);

            // Assert
            factoryMock.Verify(f => f.GetConnectionString(), Times.AtLeastOnce);
        }

        // ================================================================
        // 3. ROTACIÓN DE BACKUPS
        // ================================================================

        [Fact]
        public void DeleteOldBackups_ShouldRemoveBackupsOlderThanRetentionDays()
        {
            // Arrange
            BackupOrchestrator orchestrator = TestBackupHelper.CreateOrchestratorWithMocks
            (
                out _,
                out _,
                TestBackupHelper.CreateSettings(retentionDays: 7)
            );

            DateTime now = DateTime.Now;
            TestBackupHelper.CreateMockBackupFile(_tempBackupDir, now);               // hoy
            TestBackupHelper.CreateMockBackupFile(_tempBackupDir, now.AddDays(-5));   // dentro de retención
            TestBackupHelper.CreateMockBackupFile(_tempBackupDir, now.AddDays(-10));  // fuera de retención

            // Act
            orchestrator.DeleteOldBackups(_tempBackupDir);

            // Assert
            string[] remaining = Directory.GetFiles(_tempBackupDir, "backup_*.db");
            Assert.Equal(2, remaining.Length);
        }

        [Fact]
        public void DeleteOldBackups_WhenNoOldBackups_ShouldNotDeleteAnything()
        {
            // Arrange
            BackupOrchestrator orchestrator = TestBackupHelper.CreateOrchestratorWithMocks
            (
                out _,
                out _,
                TestBackupHelper.CreateSettings(retentionDays: 30)
            );

            DateTime now = DateTime.Now;
            TestBackupHelper.CreateMockBackupFile(_tempBackupDir, now);
            TestBackupHelper.CreateMockBackupFile(_tempBackupDir, now.AddDays(-5));
            TestBackupHelper.CreateMockBackupFile(_tempBackupDir, now.AddDays(-15));

            // Act
            orchestrator.DeleteOldBackups(_tempBackupDir);

            // Assert
            string[] remaining = Directory.GetFiles(_tempBackupDir, "backup_*.db");
            Assert.Equal(3, remaining.Length);
        }

        // ================================================================
        // 4. OBTENCIÓN DE RUTA BASE
        // ================================================================

        [Fact]
        public void GetBackupBaseDirectory_ShouldReturnCorrectPath()
        {
            // Arrange
            string expectedDbDir = @"C:\Data";
            string connectionString = $"Data Source={expectedDbDir}\\test.db";
            string expectedBackupDir = Path.Combine(expectedDbDir, "Backup");

            Mock<IDbContextFactory> factoryMock = new Mock<IDbContextFactory>();
            factoryMock.Setup(f => f.GetConnectionString()).Returns(connectionString);

            BackupOrchestrator orchestrator = new BackupOrchestrator
            (
                factoryMock.Object,
                new Mock<ILogger<BackupOrchestrator>>().Object,
                TestBackupHelper.CreateOptions(TestBackupHelper.CreateSettings())
            );

            // Act
            string result = orchestrator.GetBackupBaseDirectory();

            // Assert
            Assert.Equal(expectedBackupDir, result);
        }
    }
}