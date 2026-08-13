using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        // Consultas
        Task<User?> GetByIdAsync(int id, bool withTracking = false);
        Task<User?> GetByEmailAsync(string email);

        // Comandos
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);

        // Verificaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
    }
}