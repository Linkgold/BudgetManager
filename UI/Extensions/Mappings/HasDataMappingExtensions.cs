using Shared.DTOs.Response.HasData;
using UI.Models;

namespace UI.Extensions.Mappings
{
    public static class HasDataMappingExtensions
    {
        public static HasDataModel ToHasDataModel(this HasDataResponseDTO dto) => HasDataModel.FromDTO(dto);
    }
}