using Shared.DTOs.Response.Data;
using Shared.DTOs.Response.HasData;
using Shared.DTOs.Response.MonthDetail;

namespace Application.Interfaces
{
    public interface IDataService
    {
        Task<DashboardResponseDTO> GetDashboardDataAsync(int year);
        Task<AnnualDetailResponseDTO> GetAnnualDetailAsync(int year);
        Task<HasDataResponseDTO> GetHasDataAsync();
    }
}