using Shared.DTOs.Response;
using UI.Models;

namespace UI.Extensions.Mappings
{
    public static class BudgetMappingExtensions
    {
        public static BudgetModel ToBudgetModel(this BudgetResponseDTO dto) => BudgetModel.FromDTO(dto);

        public static BulkBudgetModel ToBulkBudgetModel(this BulkBudgetResponseDTO dto) => BulkBudgetModel.FromDTO(dto);

        public static List<BudgetModel> ToBudgetModelList(this IEnumerable<BudgetResponseDTO> dtos)
        {
            return dtos
                .Select(dto => dto.ToBudgetModel())
                .OrderBy(b => b.CategoryName)
                .ThenBy(b => b.Month)
                .ToList();
        }
    }
}
