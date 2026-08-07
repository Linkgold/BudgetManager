using Contracts.Enums;
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

        private List<DashboardMonthTotals> _monthTotals = new();
        private DashboardTotals _totals = new();
        private DashboardAccumulatedTotals _accumulatedTotals = new();

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

                CalculateTotals();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar el dashboard", ex);
                ToastService.ShowError("Error al cargar los datos del dashboard.");
            }
        }

        private void CalculateTotals()
        {
            // ✅ Totales por mes
            _monthTotals = _dashboardData.Months
                .Select(m => new DashboardMonthTotals
                {
                    Month = m.Month,
                    TotalBudget = m.TotalBudget,
                    TotalSpent = m.TotalSpent,
                    TotalDisplaySpent = m.TotalDisplaySpent
                })
                .ToList();

            // ✅ Totales generales
            _totals = new DashboardTotals
            {
                TotalBudget = _dashboardData.TotalBudget,
                TotalSpent = _dashboardData.TotalSpent,
                TotalDisplaySpent = _dashboardData.TotalDisplaySpent
            };

            // ✅ Acumulados
            decimal acumuladoBudget = 0m;
            decimal acumuladoSpent = 0m;
            decimal acumuladoDisplaySpent = 0m;

            _accumulatedTotals = new DashboardAccumulatedTotals();

            foreach (DashboardMonthColumn month in _dashboardData.Months)
            {
                acumuladoBudget += month.TotalBudget;
                acumuladoSpent += month.TotalSpent;
                acumuladoDisplaySpent += month.TotalDisplaySpent;

                _accumulatedTotals.Monthly.Add(new DashboardAccumulatedMonth
                {
                    Month = month.Month,
                    AcumuladoBudget = acumuladoBudget,
                    AcumuladoSpent = acumuladoSpent,
                    AcumuladoDisplaySpent = acumuladoDisplaySpent
                });
            }

            _accumulatedTotals.TotalBudget = acumuladoBudget;
            _accumulatedTotals.TotalSpent = acumuladoSpent;
            _accumulatedTotals.TotalDisplaySpent = acumuladoDisplaySpent;
        }
    }
}