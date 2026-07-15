using Shared.DTOs.Request;
using Shared.DTOs.Response;

namespace Application.Interfaces
{
    /// <summary>
    /// Servicio de aplicación para gestionar gastos fijos
    /// </summary>
    public interface IFixedExpenseService
    {
        // ==================== CONSULTAS ====================

        Task<FixedExpenseResponseDTO> GetByIdAsync(int id);
        Task<List<FixedExpenseResponseDTO>> GetAllAsync();
        Task<List<FixedExpenseResponseDTO>> GetByCategoryIdAsync(int categoryId);
        Task<List<FixedExpenseResponseDTO>> GetActiveAsync();
        Task<List<FixedExpenseResponseDTO>> GetActiveByCategoryIdAsync(int categoryId);
        Task<List<FixedExpenseResponseDTO>> GetActiveForPeriodAsync(int month, int year);
        Task<List<FixedExpenseResponseDTO>> GetActiveForPeriodByCategoryAsync(int categoryId, int month, int year);

        // ==================== COMANDOS ====================

        Task<FixedExpenseResponseDTO> CreateAsync(CreateFixedExpenseRequestDTO request);
        Task<FixedExpenseResponseDTO> UpdateAsync(int id, UpdateFixedExpenseRequestDTO request);
        Task DeleteAsync(int id);
        Task ActivateAsync(int id);
        Task DeactivateAsync(int id);

        // ==================== VALIDACIONES ====================

        Task<bool> ExistsAsync(int id);
        Task<bool> IsActiveAsync(int id);
        Task<decimal> GetTotalForPeriodByCategoryAsync(int categoryId, int month, int year);
    }
}
