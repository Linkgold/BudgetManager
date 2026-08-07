using Contracts.Enums;

namespace Shared.DTOs.Response
{
    /// <summary>
    /// DTO para devolver información de una transacción
    /// </summary>
    public class TransactionResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum CategoryNature { get; set; } = CategoryNatureEnum.Expense;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}