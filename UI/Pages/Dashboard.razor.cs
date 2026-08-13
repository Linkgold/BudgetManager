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
        // ================================================================
        // 1. INYECCIONES
        // ================================================================

        [Inject]
        private HasDataService HasDataService { get; set; } = default!;


        // ================================================================
        // 2. MODELOS Y ESTADO
        // ================================================================

        private readonly CacheDictionary<int, DashboardModel> _dashboardCache = new();

        private int _currentYear = DateTime.Now.Year;
        private DashboardModel _dashboardData = new();
        private List<MonthModel> _months = new();
        private List<int> _years = new();
        private HasDataModel? _hasData;
        private int? _hoverMonth = null;
        private bool _isLoading = true;

        private bool IsCurrentYearSelected => _currentYear == DateTime.Now.Year;

        // ================================================================
        // 3. CICLO DE VIDA
        // ================================================================

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

        // ================================================================
        // 4. CARGA DE DATOS
        // ================================================================

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

        // ================================================================
        // 5. NAVEGACIÓN Y HOVER
        // ================================================================

        private void NavigateToMonth(int month)
        {
            NavigationManager.NavigateTo($"/monthly/{month}/{_currentYear}");
        }

        private void SetHoverMonth(int? month)
        {
            _hoverMonth = month;

            StateHasChanged();
        }

        private string GetColumnHoverClass(int month)
        {
            return _hoverMonth == month ? "column-hover" : "";
        }

        // ================================================================
        // 6. EVENTOS
        // ================================================================

        private async Task OnYearChanged()
        {
            await LoadData();
        }

        private async Task SetCurrentYear()
        {
            _currentYear = DateTime.Now.Year;
            await LoadData();
        }
    }
}