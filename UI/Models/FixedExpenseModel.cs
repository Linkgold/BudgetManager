using Contracts.Enums;
using Shared.DTOs.Response;
using UI.Extensions;

namespace UI.Models
{
    public class FixedExpenseModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum CategoryNature { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public decimal DisplayAmount => -Amount;

        public static FixedExpenseModel FromDTO(FixedExpenseResponseDTO dto, CategoryResponseDTO? category = null)
        {
            return new FixedExpenseModel
            {
                Id = dto.Id,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName,
                CategoryNature = category?.Nature ?? CategoryNatureEnum.Expense,
                Name = dto.Name,
                Description = dto.Description,
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