namespace Infrastructure.Data.Factories.Interfaces
{
    public interface IDatabaseBackup
    {
        Task BackupAsync(string destinationPath);
    }
}