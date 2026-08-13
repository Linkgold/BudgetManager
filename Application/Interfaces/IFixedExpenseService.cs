using Shared.DTOs.Request;
using Shared.DTOs.Response;

namespace Application.Interfaces
{
    /// <summary>
    /// Servicio de aplicación para gestionar gastos fijos
    /// </summary>
    public interface IFixedExpenseService
    {
        // Consultas
        Task<FixedExpenseResponseDTO> GetByIdAsync(int id);
        Task<List<FixedExpenseResponseDTO>> GetAllByYearAsync(int year);

        // Comandos
        Task<FixedExpenseResponseDTO> CreateAsync(CreateFixedExpenseRequestDTO request);
        Task<FixedExpenseResponseDTO> UpdateAsync(int id, UpdateFixedExpenseRequestDTO request);
        Task DeleteAsync(int id);

        // Verificaciones
        Task<bool> ExistsAsync(int id);
    }
}