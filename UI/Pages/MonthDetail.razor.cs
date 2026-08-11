using Microsoft.AspNetCore.Components;
using System.Globalization;
using UI.Extensions;
using UI.Models.Cache;
using UI.Models.MonthDetail;
using UI.Services;
using UI.Shared;

namespace UI.Pages
{
    public partial class MonthDetail : BasePage
    {
        [Parameter]
        public int Month { get; set; }

        [Parameter]
        public int Year { get; set; }

        private readonly CacheDictionary<int, AnnualDetailModel> _annualCache = new();

        private AnnualDetailModel? _annualData;
        private MonthDetailModel? _currentMonthData;
        private bool _isLoading = true;
        private Dictionary<string, bool> _expandedCategories = new();

        [Inject]
        private HasDataService HasDataService { get; set; } = default!;

        protected override async Task OnParametersSetAsync()
        {
            if (Month == 0) Month = DateTime.Now.Month;
            if (Year == 0) Year = DateTime.Now.Year;

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _isLoading = true;

                // 🔥 Intentar obtener del caché
                if (_annualCache.TryGetValue(Year, out List<AnnualDetailModel>? cachedList) &&
                    cachedList != null && cachedList.Count == 1)
                {
                    _annualData = cachedList[0];
                    _currentMonthData = _annualData.GetMonth(Month);
                    InitializeExpandedCategories();
                    StateHasChanged();
                    return;
                }

                // 🔥 Si no está en caché, cargar de la API
                AnnualDetailModel? data = await APIService.GetAnnualDetailAsync(Year);

                if (data != null)
                {
                    _annualData = data;
                    _annualCache.Add(Year, new List<AnnualDetailModel> { data });
                    _currentMonthData = _annualData.GetMonth(Month);
                    InitializeExpandedCategories();
                }
                else
                {
                    _currentMonthData = new MonthDetailModel();
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar los datos del mes {Month}/{Year}", ex);
                ToastService.ShowError("Error al cargar los datos del mes.");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void InitializeExpandedCategories()
        {
            _expandedCategories.Clear();

            if (_currentMonthData != null)
            {
                foreach (MonthDetailCategoryModel category in _currentMonthData.Categories)
                {
                    _expandedCategories[category.CategoryName] = false;
                }
            }
        }

        private void ToggleExpand(string categoriaNombre)
        {
            if (_expandedCategories.ContainsKey(categoriaNombre))
            {
                _expandedCategories[categoriaNombre] = !_expandedCategories[categoriaNombre];
            }
            else
            {
                _expandedCategories[categoriaNombre] = true;
            }
            StateHasChanged();
        }

        private string FormatCurrency(decimal amount)
        {
            return amount.ToString("C", new CultureInfo("es-ES"));
        }

        private async Task PreviousMonth()
        {
            if (Month == 1)
            {
                Month = 12;
                Year--;
            }
            else
            {
                Month--;
            }

            // 🔥 Actualizar el mes actual desde los datos en caché
            _currentMonthData = _annualData?.GetMonth(Month) ?? new MonthDetailModel();
            InitializeExpandedCategories();
            StateHasChanged();

            await UpdateUrlAsync();
        }

        private async Task NextMonth()
        {
            if (Month == 12)
            {
                Month = 1;
                Year++;
            }
            else
            {
                Month++;
            }

            // 🔥 Actualizar el mes actual desde los datos en caché
            _currentMonthData = _annualData?.GetMonth(Month) ?? new MonthDetailModel();
            InitializeExpandedCategories();
            StateHasChanged();

            await UpdateUrlAsync();
        }

        private async Task UpdateUrlAsync()
        {
            NavigationManager.NavigateTo($"/monthly/{Month}/{Year}", replace: true);
        }
    }
}