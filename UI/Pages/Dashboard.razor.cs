using UI.Helpers;
using UI.Models;
using UI.Models.Dashboard;
using UI.Shared;
using UI.Extensions;

namespace UI.Pages
{
    public partial class Dashboard : BasePage
    {
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
                DashboardModel? data = await APIService.GetDashboardDataAsync(_currentYear);

                _dashboardData = data != null ? data : new DashboardModel();

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