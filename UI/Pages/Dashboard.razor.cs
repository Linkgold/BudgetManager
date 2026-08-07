using Microsoft.AspNetCore.Components;
using UI.Helpers;
using UI.Models;
using UI.Models.Dashboard;
using UI.Services.Pages;
using UI.Shared;

namespace UI.Pages
{
    public partial class Dashboard : BasePage
    {
        [Inject]
        private DashboardService DashboardService { get; set; } = default!;

        private int _currentYear = DateTime.Now.Year;
        private DashboardModel _dashboardData = new();
        private List<MonthModel> _months = new();

        protected override async Task OnInitializedAsync()
        {
            _months = MonthHelper.GetMonthsWithShortName();
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                _dashboardData = await DashboardService.GetDashboardDataAsync(_currentYear);

                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar el dashboard", ex);
                ToastService.ShowError("Error al cargar los datos del dashboard.");
            }
        }
    }
}