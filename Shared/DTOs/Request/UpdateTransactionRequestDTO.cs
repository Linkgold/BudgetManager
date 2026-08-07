using Contracts.Enums;

namespace Shared.DTOs.Request
{
    /// <summary>
    /// DTO para actualizar una transacción existente
    /// </summary>
    public class UpdateTransactionRequestDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public string Currency { get; set; } = "EUR";
        public DateTime Date { get; set; }
    }
}