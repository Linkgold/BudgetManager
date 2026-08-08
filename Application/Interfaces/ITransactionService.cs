using Shared.DTOs.Request;
using Shared.DTOs.Response;

namespace Application.Interfaces
{
    /// <summary>
    /// Servicio de aplicación para gestionar transacciones
    /// </summary>
    public interface ITransactionService
    {
        // Consultas
        Task<TransactionResponseDTO> GetByIdAsync(int id);
        Task<List<TransactionResponseDTO>> GetAllByYearAsync(int year);

        // Comandos
        Task<TransactionResponseDTO> CreateAsync(CreateTransactionRequestDTO request);
        Task<TransactionResponseDTO> UpdateAsync(int id, UpdateTransactionRequestDTO request);
        Task DeleteAsync(int id);

        // Verificaciones
        Task<bool> ExistsAsync(int id);
    }
}