using Domain.Enums;

namespace Application.DTOs.Request
{
    /// <summary>
    /// DTO para crear una nueva transacción
    /// </summary>
    public class CreateTransactionRequestDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionTypeEnum Type { get; set; }
        public DateTime Date { get; set; }
    }
}