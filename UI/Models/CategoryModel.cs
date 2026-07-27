using Contracts.Enums;
using Shared.DTOs.Response;

namespace UI.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CategoryNatureEnum Nature { get; set; }

        public static CategoryModel FromDTO(CategoryResponseDTO dto)
        {
            return new CategoryModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Nature = dto.Nature
            };
        }
    }
}