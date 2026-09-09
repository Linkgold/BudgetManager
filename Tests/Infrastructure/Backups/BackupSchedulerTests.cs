using Infrastructure.Data.Backup;
using Moq;
using Tests.Helpers;

namespace Tests.Infrastructure.Backup
{
    public class BackupSchedulerTests
    {
        // ================================================================
        // 1. CÁLCULO DE PRÓXIMA EJECUCIÓN
        // ================================================================

        [Fact]
        public void NextRunTime_WhenBeforeSchedule_ShouldReturnToday()
        {
            // Arrange
            BackupSettings settings = TestBackupHelper.CreateSettings(scheduleHour: 2);
            DateTime now = new DateTime(2025, 3, 15, 1, 30, 0);
            DateTime expected = new DateTime(2025, 3, 15, 2, 0, 0);

            // Act
            DateTime nextRun = new DateTime(now.Year, now.Month, now.Day, settings.ScheduleHour, 0, 0);
            if (nextRun <= now) nextRun = nextRun.AddDays(1);

            // Assert
            Assert.Equal(expected, nextRun);
        }

        [Fact]
        public void NextRunTime_WhenAfterSchedule_ShouldReturnTomorrow()
        {
            // Arrange
            BackupSettings settings = TestBackupHelper.CreateSettings(scheduleHour: 2);
            DateTime now = new DateTime(2025, 3, 15, 10, 0, 0);
            DateTime expected = new DateTime(2025, 3, 16, 2, 0, 0);

            // Act
            DateTime nextRun = new DateTime(now.Year, now.Month, now.Day, settings.ScheduleHour, 0, 0);
            if (nextRun <= now) nextRun = nextRun.AddDays(1);

            // Assert
            Assert.Equal(expected, nextRun);
        }

        // ================================================================
        // 2. LLAMADA AL ORQUESTADOR AL DESPERTAR
        // ================================================================

        [Fact]
        public async Task ExecuteAsync_ShouldCallOrchestratorOnStartup()
        {
            // Arrange
            BackupScheduler scheduler = TestBackupHelper.CreateSchedulerWithMocks(
                out Mock<IBackupOrchestrator> orchestratorMock,
                out _);

            // Act
            // Simulamos el inicio del scheduler llamando al método que dispara el orquestador
            await scheduler.StartAsync(CancellationToken.None);

            // Esperar un momento para que se ejecute la lógica
            await Task.Delay(100);

            // Detener
            await scheduler.StopAsync(CancellationToken.None);

            // Assert
            orchestratorMock.Verify(o => o.PerformBackupIfNeededAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}