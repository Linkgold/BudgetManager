using Shared.DTOs.Response;

namespace UI.Models
{
    public class BulkBudgetModel
    {
        public int CategoryId { get; set; }
        public int Year { get; set; }
        public List<int> AffectedIds { get; set; } = new List<int>();
        public int TotalCreated { get; set; }

        public static BulkBudgetModel FromDTO(BulkBudgetResponseDTO dto)
        {
            return new BulkBudgetModel
            {
                CategoryId = dto.CategoryId,
                Year = dto.Year,
                AffectedIds = dto.AfectedIds,
                TotalCreated = dto.TotalCreated
            };
        }
    }
}