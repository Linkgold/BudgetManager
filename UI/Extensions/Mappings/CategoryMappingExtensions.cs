using Contracts.Enums;
using Shared.DTOs.Response;
using UI.Models;

namespace UI.Extensions.Mappings
{
    public static class CategoryMappingExtensions
    {
        public static CategoryModel ToCategoryModel(this CategoryResponseDTO dto) => CategoryModel.FromDTO(dto);
        public static List<CategoryModel> ToCategoryModelList(this IEnumerable<CategoryResponseDTO> dtos)
        {
            return dtos.Select(dto => dto.ToCategoryModel()).OrderBy(c => c.Name).ToList();
        }

        public static List<CategoryModel> ToExpenseMixedCategoryModelList(this IEnumerable<CategoryModel> models)
        {
            return models
                .Where(c => c.Nature == CategoryNatureEnum.Expense || c.Nature == CategoryNatureEnum.Mixed)
                .OrderBy(c => c.Name)
                .ToList();
        }
    }
}