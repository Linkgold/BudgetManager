using Contracts.Enums;
using Shared.DTOs.Response;

namespace UI.Models
{
    public class BudgetModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum CategoryNature { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static BudgetModel FromDTO(BudgetResponseDTO dto, CategoryResponseDTO? category = null)
        {
            return new BudgetModel
            {
                Id = dto.Id,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                CategoryNature = category?.Nature ?? CategoryNatureEnum.Expense,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Year = dto.Year,
                Month = dto.Month,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}