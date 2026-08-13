using Shared.DTOs.Request;
using Shared.DTOs.Response;

namespace Application.Interfaces
{
    /// <summary>
    /// Servicio de aplicación para gestionar presupuestos
    /// </summary>
    public interface IBudgetService
    {
        // Consultas
        Task<BudgetResponseDTO> GetByIdAsync(int id);
        Task<List<BudgetResponseDTO>> GetAllByYearAsync(int year);

        // Comandos
        Task<BulkBudgetResponseDTO> CreateBulkAsync(CreateBulkBudgetRequestDTO request);
        Task<BudgetResponseDTO> CreateAsync(CreateBudgetRequestDTO request);
        Task<BulkBudgetResponseDTO> UpdateBulkAsync(UpdateBulkBudgetRequestDTO request);
        Task<BudgetResponseDTO> UpdateAsync(int id, UpdateBudgetRequestDTO request);
        Task<BulkBudgetResponseDTO> DeleteBulkAsync(DeleteBulkBudgetRequestDTO request);
        Task DeleteAsync(int id);

        // Verificaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsForCategoryAndPeriodAsync(int categoryId, int month, int year);
    }
}