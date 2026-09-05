using Microsoft.AspNetCore.Components;
using System.Globalization;
using UI.Extensions;
using UI.Models;
using UI.Models.Cache;
using UI.Models.Forms;
using UI.Models.MonthDetail;
using UI.Services;
using UI.Shared;

namespace UI.Pages
{
    public partial class MonthDetail : BasePage
    {
        [Inject]
        private HasDataService HasDataService { get; set; } = default!;

        [Parameter]
        public int Month { get; set; }

        [Parameter]
        public int Year { get; set; }

        private readonly CacheDictionary<int, AnnualDetailModel> _annualCache = new();

        private AnnualDetailModel? _annualData;
        private MonthDetailModel? _currentMonthData;
        private bool _isLoading = true;
        private Dictionary<string, bool> _expandedCategories = new();
        private Dictionary<int, bool> _expandedDays = new();
        private List<CategoryModel> _categories = new();

        private enum ViewModeEnum
        {
            Category,
            Day
        }

        private ViewModeEnum ViewMode = ViewModeEnum.Category;

        // Método para agrupar transacciones por día
        private Dictionary<int, List<MonthDetailTransactionModel>> GetTransactionsByDay()
        {
            if (_currentMonthData == null) return new();

            return _currentMonthData.Categories
                .SelectMany(c => c.Transactions)
                .GroupBy(t => t.Date.Day)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Date).ToList());
        }

        private void SetViewMode(ViewModeEnum mode)
        {
            ViewMode = mode;
            StateHasChanged();
        }

        // Modal
        private bool _isTransactionModalOpen = false;
        private FormModeEnum _modalMode;
        private TransactionModel? _transactionToModal = null;


        protected override async Task OnParametersSetAsync()
        {
            if (Month == 0) Month = DateTime.Now.Month;
            if (Year == 0) Year = DateTime.Now.Year;

            await LoadCategories();
            await LoadDataAsync();
        }

        private async Task LoadCategories()
        {
            _categories = await APIService.GetCategoriesAsync() ?? new List<CategoryModel>();
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
                    InitializeExpandedDays();
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

        private void InitializeExpandedDays()
        {
            _expandedDays.Clear();

            Dictionary<int, List<MonthDetailTransactionModel>> days = GetTransactionsByDay();
            foreach (int day in days.Keys)
            {
                _expandedDays[day] = true;  // ✅ Todos desplegados
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

        private void ToggleDay(int day)
        {
            if (_expandedDays.ContainsKey(day))
                _expandedDays[day] = !_expandedDays[day];
            else
                _expandedDays[day] = true;

            StateHasChanged();
        }

        private bool IsDayExpanded(int day) => _expandedDays.ContainsKey(day) && _expandedDays[day];

        // ================================================================
        // NAVEGACIÓN
        // ================================================================

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
            InitializeExpandedDays();
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
            InitializeExpandedDays();
            StateHasChanged();

            await UpdateUrlAsync();
        }

        private async Task UpdateUrlAsync()
        {
            NavigationManager.NavigateTo($"/monthly/{Month}/{Year}", replace: true);
        }

        // ================================================================
        // MODAL DE TRANSACCIONES
        // ================================================================

        private void OpenCreateTransactionModal()
        {
            DateTime defaultDate = new DateTime(Year, Month, 1);

            _transactionToModal = new TransactionModel
            {
                Date = defaultDate,
                Amount = 0m
            };

            _modalMode = FormModeEnum.Create;
            _isTransactionModalOpen = true;
            StateHasChanged();
        }

        private void OpenEditTransactionModal(TransactionModel transaction)
        {
            _transactionToModal = transaction;
            _modalMode = FormModeEnum.Edit;
            _isTransactionModalOpen = true;
            StateHasChanged();
        }

        private void OpenDeleteTransactionModal(TransactionModel transaction)
        {
            _transactionToModal = transaction;
            _modalMode = FormModeEnum.Delete;
            _isTransactionModalOpen = true;
            StateHasChanged();
        }

        private async Task OnTransactionModalSaved()
        {
            _isTransactionModalOpen = false;
            _transactionToModal = null;

            // Refrescar caché de HasData
            await HasDataService.RefreshAsync();

            // Recargar datos (forzar refresco de caché anual)
            _annualCache.Remove(Year);
            await LoadDataAsync();
            StateHasChanged();
        }

        private async Task OnTransactionModalCancelled()
        {
            _isTransactionModalOpen = false;
            _transactionToModal = null;
            StateHasChanged();  // Solo actualizar la UI sin recargar datos
        }

        // ================================================================
        // MÉTODO PARA ENCONTRAR UNA TRANSACCIÓN
        // ================================================================

        private TransactionModel? FindTransaction(int transactionId)
        {
            if (_currentMonthData == null) return null;

            foreach (MonthDetailCategoryModel category in _currentMonthData.Categories)
            {
                MonthDetailTransactionModel? transaction = category.Transactions.FirstOrDefault(t => t.Id == transactionId);
                if (transaction != null)
                {
                    return new TransactionModel
                    {
                        Id = transaction.Id,
                        CategoryId = category.CategoryId,
                        CategoryName = category.CategoryName,
                        CategoryNature = category.Nature,
                        TransactionType = transaction.TransactionType,
                        Name = transaction.Name,
                        Description = transaction.Description,
                        Amount = transaction.Amount,
                        Date = transaction.Date
                    };
                }
            }

            return null;
        }
    }
}