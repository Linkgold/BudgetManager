using Shared.DTOs.Response;
using UI.Models;

namespace UI.Extensions
{
    public static class FixedExpenseMappingExtensions
    {
        public static FixedExpenseModel ToFixedExpenseModel(this FixedExpenseResponseDTO dto) => FixedExpenseModel.FromDTO(dto);

        public static List<FixedExpenseModel> ToFixedExpenseModelList(this IEnumerable<FixedExpenseResponseDTO> dtos)
        {
            return dtos
                .Select(dto => dto.ToFixedExpenseModel())
                .OrderBy(f => f.CategoryName)
                .ThenBy(f => f.Month)
                .ToList();
        }
    }
}
