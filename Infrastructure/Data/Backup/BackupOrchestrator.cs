using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data.Factories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Data.Backup
{
    public class BackupOrchestrator : IBackupOrchestrator
    {
        private readonly IDbContextFactory _factory;
        private readonly ILogger<BackupOrchestrator> _logger;
        private readonly BackupSettings _settings;

        public BackupOrchestrator(
            IDbContextFactory factory,
            ILogger<BackupOrchestrator> logger,
            IOptions<BackupSettings> settings)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task PerformBackupIfNeededAsync(CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            DateTime todayScheduled = new DateTime(now.Year, now.Month, now.Day, _settings.ScheduleHour, 0, 0);

            // Si es antes de la hora programada, no ejecutar
            if (now < todayScheduled)
            {
                _logger.LogInformation("Aún no es hora de backup (programado a las {Hour}:00). No se ejecuta.", _settings.ScheduleHour);
                return;
            }

            string backupDirectory = GetBackupBaseDirectory();

            if (!Directory.Exists(backupDirectory))
            {
                _logger.LogInformation("No existe carpeta de backups. Creando primer backup...");
                await PerformBackupAsync(cancellationToken);
                return;
            }

            string[] backupFiles = Directory.GetFiles(backupDirectory, "backup_*.db");

            if (backupFiles.Length == 0)
            {
                _logger.LogInformation("No hay backups previos. Creando primer backup...");
                await PerformBackupAsync(cancellationToken);
                return;
            }

            string latestBackup = backupFiles
                .OrderByDescending(file => File.GetCreationTime(file))
                .First();

            DateTime latestDate = File.GetCreationTime(latestBackup).Date;
            DateTime today = now.Date;

            if (latestDate < today)
            {
                _logger.LogInformation("Último backup: {LatestDate}. No hay backup de hoy. Ejecutando...", latestDate.ToShortDateString());
                await PerformBackupAsync(cancellationToken);
            }
            else
            {
                _logger.LogInformation("Ya existe backup de hoy ({Today}). No se ejecuta nuevo backup.", today.ToShortDateString());
            }
        }

        public async Task PerformBackupAsync(CancellationToken cancellationToken)
        {
            IDatabaseBackup backup = _factory as IDatabaseBackup
                ?? throw new InvalidOperationException("La fábrica no implementa IDatabaseBackup.");

            string baseDirectory = GetBackupBaseDirectory();

            if (!Directory.Exists(baseDirectory))
            {
                Directory.CreateDirectory(baseDirectory);
            }

            string fileName = string.Format(_settings.FileNamePattern, DateTime.Now);
            string destinationPath = Path.Combine(baseDirectory, fileName);

            _logger.LogInformation("Iniciando backup en: {DestinationPath}", destinationPath);
            await backup.BackupAsync(destinationPath);
            _logger.LogInformation("Backup completado: {DestinationPath}", destinationPath);

            DeleteOldBackups(baseDirectory);
        }

        public string GetBackupBaseDirectory()
        {
            string connectionString = _factory.GetConnectionString();

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La fábrica no devolvió una cadena de conexión válida.");
            }

            Microsoft.Data.Sqlite.SqliteConnectionStringBuilder builder =
                new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
            string dataSource = builder.DataSource;

            if (string.IsNullOrEmpty(dataSource))
            {
                throw new InvalidOperationException("No se pudo obtener el DataSource desde la cadena de conexión.");
            }

            string directory = Path.GetDirectoryName(dataSource) ?? string.Empty;

            if (string.IsNullOrEmpty(directory))
            {
                throw new InvalidOperationException("No se pudo determinar el directorio de la base de datos.");
            }

            return Path.Combine(directory, _settings.BackupDirectory);
        }

        public void DeleteOldBackups(string backupDirectory)
        {
            try
            {
                DateTime cutoff = DateTime.Now.AddDays(-_settings.RetentionDays);

                string[] files = Directory.GetFiles(backupDirectory, "backup_*.db");

                foreach (string file in files)
                {
                    DateTime fileCreationTime = File.GetCreationTime(file);
                    if (fileCreationTime < cutoff)
                    {
                        File.Delete(file);
                        _logger.LogInformation("Backup antiguo eliminado: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al eliminar backups antiguos en {BackupDirectory}", backupDirectory);
            }
        }
    }
}