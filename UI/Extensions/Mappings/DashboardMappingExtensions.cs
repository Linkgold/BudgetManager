using Shared.DTOs.Response.Data;
using UI.Models.Dashboard;

namespace UI.Extensions.Mappings
{
    public static class DashboardMappingExtensions
    {
        public static DashboardModel ToDashboardModel(this DashboardResponseDTO dto) => DashboardModel.FromDTO(dto);
    }
}