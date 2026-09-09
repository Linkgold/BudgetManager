using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Factories.Interfaces
{
    /// <summary>
    /// Interfaz para la fábrica de DbContext
    /// Permite cambiar la implementación de la base de datos fácilmente
    /// </summary>
    public interface IDbContextFactory
    {
        /// <summary>
        /// Crea una nueva instancia del DbContext
        /// </summary>
        ApplicationDbContext CreateDbContext();

        /// <summary>
        /// Obtiene las opciones de configuración del DbContext
        /// </summary>
        DbContextOptions<ApplicationDbContext> GetOptions();

        /// <summary>
        /// Obtiene la cadena de conexión de la base de datos
        /// </summary>
        /// <returns></returns>
        string GetConnectionString();
    }
}