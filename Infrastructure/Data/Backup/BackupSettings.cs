namespace Infrastructure.Data.Backup
{
    public class BackupSettings
    {
        public int ScheduleHour { get; set; } = 2;
        public string BackupDirectory { get; set; } = "Backup";
        public string FileNamePattern { get; set; } = "backup_{0:yyyy-MM-dd_HHmmss}.db";
        public int RetentionDays { get; set; } = 30;
    }
}
