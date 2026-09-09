using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Data.Backup
{
    public class BackupScheduler : BackgroundService
    {
        private readonly IBackupOrchestrator _orchestrator;
        private readonly ILogger<BackupScheduler> _logger;
        private readonly BackupSettings _settings;

        public BackupScheduler
        (
            IBackupOrchestrator orchestrator,
            ILogger<BackupScheduler> logger,
            IOptions<BackupSettings> settings
        )
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackupScheduler iniciado.");

            // Backup al despertar
            try
            {
                await _orchestrator.PerformBackupIfNeededAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar backup al despertar.");
            }

            // Bucle diario
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    DateTime nextRun = new DateTime(now.Year, now.Month, now.Day, _settings.ScheduleHour, 0, 0);

                    if (nextRun <= now)
                    {
                        nextRun = nextRun.AddDays(1);
                    }

                    TimeSpan delay = nextRun - now;
                    _logger.LogInformation("Próximo backup programado para las {Hour}:00.", _settings.ScheduleHour);

                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await _orchestrator.PerformBackupAsync(stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el ciclo del BackupScheduler. Reintento en 1 hora.");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("BackupScheduler detenido.");
        }
    }
}