using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Interfaces
{
    /// <summary>
    /// Servicio de aplicación para gestionar transacciones
    /// </summary>
    public interface ITransactionService
    {
        // ==================== CONSULTAS ====================

        Task<TransactionResponseDTO> GetByIdAsync(int id);
        Task<List<TransactionResponseDTO>> GetAllAsync();
        Task<List<TransactionResponseDTO>> GetByCategoryIdAsync(int categoryId);
        Task<List<TransactionResponseDTO>> GetByMonthlyPeriodAsync(int month, int year);
        Task<List<TransactionResponseDTO>> GetByCategoryAndMonthlyPeriodAsync(int categoryId, int month, int year);
        Task<List<TransactionResponseDTO>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalByCategoryAndMonthlyPeriodAsync(int categoryId, int month, int year);

        // ==================== COMANDOS ====================

        Task<TransactionResponseDTO> CreateAsync(CreateTransactionRequestDTO request);
        Task<TransactionResponseDTO> UpdateAsync(int id, UpdateTransactionRequestDTO request);
        Task DeleteAsync(int id);

        // ==================== VERIFICACIONES ====================

        Task<bool> ExistsAsync(int id);
    }
}