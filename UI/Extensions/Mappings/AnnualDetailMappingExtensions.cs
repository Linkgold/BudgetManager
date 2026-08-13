using Shared.DTOs.Response.MonthDetail;
using UI.Models.MonthDetail;

namespace UI.Extensions.Mappings
{
    public static class AnnualDetailMappingExtensions
    {
        public static AnnualDetailModel ToAnnualDetailModel(this AnnualDetailResponseDTO dto) => AnnualDetailModel.FromDTO(dto);
    }
}
