using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Interfaces
{
    /// <summary>
    /// Servicio de aplicación para gestionar gastos fijos
    /// </summary>
    public interface IFixedExpenseService
    {
        // ==================== CONSULTAS ====================

        Task<FixedExpenseResponseDto> GetByIdAsync(int id);
        Task<List<FixedExpenseResponseDto>> GetAllAsync();
        Task<List<FixedExpenseResponseDto>> GetByCategoryIdAsync(int categoryId);
        Task<List<FixedExpenseResponseDto>> GetActiveAsync();
        Task<List<FixedExpenseResponseDto>> GetActiveByCategoryIdAsync(int categoryId);
        Task<List<FixedExpenseResponseDto>> GetActiveForPeriodAsync(int year, int month);
        Task<List<FixedExpenseResponseDto>> GetActiveForPeriodByCategoryAsync(int categoryId, int year, int month);

        // ==================== COMANDOS ====================

        Task<FixedExpenseResponseDto> CreateAsync(CreateFixedExpenseRequestDto request);
        Task<FixedExpenseResponseDto> UpdateAsync(int id, UpdateFixedExpenseRequestDto request);
        Task DeleteAsync(int id);
        Task ActivateAsync(int id);
        Task DeactivateAsync(int id);

        // ==================== VALIDACIONES ====================

        Task<bool> ExistsAsync(int id);
        Task<bool> IsActiveAsync(int id);
        Task<decimal> GetTotalForPeriodByCategoryAsync(int categoryId, int year, int month);
    }
}
