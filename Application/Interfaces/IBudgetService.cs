using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Interfaces
{
    /// <summary>
    /// Servicio de aplicación para gestionar presupuestos
    /// </summary>
    public interface IBudgetService
    {
        // ==================== CONSULTAS ====================
        Task<BudgetResponseDTO> GetByIdAsync(int id);
        Task<List<BudgetResponseDTO>> GetAllAsync();
        Task<List<BudgetResponseDTO>> GetByCategoryIdAsync(int categoryId);
        Task<List<BudgetResponseDTO>> GetByPeriodAsync(int month, int year);
        Task<BudgetResponseDTO> GetByCategoryAndPeriodAsync(int categoryId, int month, int year);

        // ==================== RESUMEN (con cálculos) ====================
        Task<BudgetSummaryDTO> GetSummaryByCategoryAndPeriodAsync(int categoryId, int month, int year);

        // ==================== COMANDOS ====================
        Task<BudgetResponseDTO> CreateAsync(CreateBudgetRequestDTO request);
        Task<BudgetResponseDTO> UpdateAsync(int id, UpdateBudgetRequestDTO request);
        Task DeleteAsync(int id);

        // ==================== VERIFICACIONES ====================
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsForCategoryAndPeriodAsync(int categoryId, int month, int year);
    }
}