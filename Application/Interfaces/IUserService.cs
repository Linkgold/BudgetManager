using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Interfaces
{
    public interface IUserService
    {
        // ==================== CONSULTAS ====================
        Task<UserResponseDTO> GetByIdAsync(int id);
        Task<UserResponseDTO> GetByEmailAsync(string email);
        Task<UserResponseDTO> GetCurrentUserAsync();

        // ==================== COMANDOS ====================
        Task<UserResponseDTO> CreateAsync(CreateUserRequestDTO request);
        Task<UserResponseDTO> UpdateAsync(UpdateUserRequestDTO request);
        Task DeleteAsync();

        // ==================== AUTENTICACIÓN ====================
        Task<string> LoginAsync(string email, string password);
        Task<bool> ValidatePasswordAsync(string email, string password);
        Task ChangePasswordAsync(string currentPassword, string newPassword);

        // ==================== VERIFICACIONES ====================
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
