using Microsoft.AspNetCore.Components;
using UI.Extensions;
using UI.Helpers;
using UI.Models;
using UI.Models.Cache;
using UI.Models.Dashboard;
using UI.Services;
using UI.Shared;

namespace UI.Pages
{
    public partial class Dashboard : BasePage
    {
        [Inject]
        private HasDataService HasDataService { get; set; } = default!;

        private readonly CacheDictionary<int, DashboardModel> _dashboardCache = new();

        private int _currentYear = DateTime.Now.Year;
        private DashboardModel _dashboardData = new();
        private List<MonthModel> _months = new();
        private List<int> _years = new();
        private HasDataModel? _hasData;
        private bool _isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            _months = MonthHelper.GetMonthsWithShortName();
            _years = new List<int>();
            for (int year = 2020; year <= 2050; year++)
            {
                _years.Add(year);
            }

            _hasData = await HasDataService.GetDataAsync();

            await LoadData();

            _isLoading = false;
        }

        private async Task LoadData()
        {
            try
            {
                if (_dashboardCache.TryGetValue(_currentYear, out List<DashboardModel>? cachedList) &&
                    cachedList != null && cachedList.Count == 1)
                {
                    _dashboardData = cachedList[0];
                    StateHasChanged();
                    return;
                }

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

        private async Task OnYearChanged()
        {
            await LoadData();
        }

        private void NavigateToMonth(int month)
        {
            NavigationManager.NavigateTo($"/monthly/{month}/{_currentYear}");
        }

        private int? _hoverMonth = null;

        private void SetHoverMonth(int? month)
        {
            _hoverMonth = month;
            StateHasChanged();
        }

        private string GetColumnHoverClass(int month)
        {
            return _hoverMonth == month ? "column-hover" : "";
        }
    }
}