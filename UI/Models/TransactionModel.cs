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
        public CategoryNatureEnum CategoryNature { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }  // Importe positivo (siempre)
        public DateTime Date { get; set; }

        // ✅ Propiedad calculada para mostrar en UI
        public decimal DisplayAmount
        {
            get
            {
                return CategoryNature switch
                {
                    CategoryNatureEnum.Income => Math.Abs(Amount),
                    CategoryNatureEnum.Expense => -Math.Abs(Amount),
                    CategoryNatureEnum.Mixed => Amount > 0 ? Math.Abs(Amount) : -Math.Abs(Amount),
                    _ => Math.Abs(Amount)
                };
            }
        }

        public static TransactionModel FromDTO(TransactionResponseDTO dto)
        {
            return new TransactionModel
            {
                Id = dto.Id,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                CategoryNature = dto.CategoryNature,
                Name = dto.Name,
                Description = dto.Description,
                Amount = dto.Amount,
                Date = dto.Date
            };
        }
    }
}