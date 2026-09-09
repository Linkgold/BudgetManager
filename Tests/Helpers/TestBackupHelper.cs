using Infrastructure.Data.Backup;
using Infrastructure.Data.Factories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.Helpers
{
    public static class TestBackupHelper
    {
        public static string CreateTempDirectory(string prefix = "BackupTests")
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"{prefix}_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            return tempDir;
        }

        public static void CleanupTempDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                try { Directory.Delete(directory, recursive: true); }
                catch { /* Ignorar errores de limpieza */ }
            }
        }

        public static string CreateMockBackupFile(string backupDirectory, DateTime creationDate, string pattern = "backup_{0:yyyy-MM-dd_HHmmss}.db")
        {
            string fileName = string.Format(pattern, creationDate);
            string filePath = Path.Combine(backupDirectory, fileName);

            File.Create(filePath).Dispose();

            try { File.SetCreationTime(filePath, creationDate); } catch { }

            return filePath;
        }

        public static BackupSettings CreateSettings
        (
            string? backupDirectory = null,
            int scheduleHour = 2,
            int retentionDays = 30,
            string pattern = "backup_{0:yyyy-MM-dd_HHmmss}.db"
        )
        {
            return new BackupSettings
            {
                ScheduleHour = scheduleHour,
                BackupDirectory = backupDirectory ?? "Backup",
                FileNamePattern = pattern,
                RetentionDays = retentionDays
            };
        }

        public static IOptions<BackupSettings> CreateOptions(BackupSettings? settings = null) => Options.Create(settings ?? CreateSettings());

        public static BackupOrchestrator CreateOrchestratorWithMocks
        (
            out Mock<IDbContextFactory> factoryMock,
            out Mock<ILogger<BackupOrchestrator>> loggerMock,
            BackupSettings? settings = null
        )
        {
            settings ??= CreateSettings();
            factoryMock = new Mock<IDbContextFactory>();
            loggerMock = new Mock<ILogger<BackupOrchestrator>>();

            factoryMock.Setup(f => f.GetConnectionString()).Returns("Data Source=C:\\Data\\test.db");

            return new BackupOrchestrator(factoryMock.Object, loggerMock.Object, CreateOptions(settings));
        }

        public static BackupScheduler CreateSchedulerWithMocks
        (
            out Mock<IBackupOrchestrator> orchestratorMock,
            out Mock<ILogger<BackupScheduler>> loggerMock,
            BackupSettings? settings = null
        )
        {
            settings ??= CreateSettings();
            orchestratorMock = new Mock<IBackupOrchestrator>();
            loggerMock = new Mock<ILogger<BackupScheduler>>();

            return new BackupScheduler(orchestratorMock.Object, loggerMock.Object, CreateOptions(settings));
        }

        public static BackupOrchestrator CreateRealOrchestrator(IDbContextFactory factory, BackupSettings? settings = null)
        {
            settings ??= CreateSettings();

            return new BackupOrchestrator(factory, new NullLogger<BackupOrchestrator>(), CreateOptions(settings));
        }
    }
}