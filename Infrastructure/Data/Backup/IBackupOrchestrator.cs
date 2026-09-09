namespace Infrastructure.Data.Backup
{
    /// <summary>
    /// Orquestador de la lógica de backups (independiente del worker)
    /// </summary>
    public interface IBackupOrchestrator
    {
        /// <summary>
        /// Comprueba si debe ejecutarse un backup hoy y lo ejecuta si es necesario
        /// </summary>
        Task PerformBackupIfNeededAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Ejecuta un backup inmediato
        /// </summary>
        Task PerformBackupAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Elimina backups antiguos según la retención configurada
        /// </summary>
        void DeleteOldBackups(string backupDirectory);

        /// <summary>
        /// Obtiene el directorio base donde se almacenan los backups
        /// </summary>
        string GetBackupBaseDirectory();
    }
}