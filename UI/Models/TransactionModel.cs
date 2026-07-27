using Contracts.Enums;
using Shared.DTOs.Response;

namespace UI.Models
{
    public class TransactionModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }  // Importe positivo (siempre)
        public DateTime Date { get; set; }

        public static TransactionModel FromDTO(TransactionResponseDTO dto)
        {
            return new TransactionModel
            {
                Id = dto.Id,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                Name = dto.Name,
                Description = dto.Description,
                Amount = dto.Amount,
                Date = dto.Date
            };
        }
    }
}