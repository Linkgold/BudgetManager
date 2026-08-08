using Shared.DTOs.Response.Dashboard;

namespace Shared.DTOs.Response.Data
{
    public class DashboardResponseDTO
    {
        public List<DashboardCategoryDTO> Categories { get; set; } = new();
        public List<DashboardMonthTotalsDTO> Months { get; set; } = new();
    }
}