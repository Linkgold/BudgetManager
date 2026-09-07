using Microsoft.AspNetCore.Components;
using UI.Extensions;
using UI.Models;
using UI.Models.Cache;
using UI.Models.Forms;
using UI.Models.MonthDetail;
using UI.Services;
using UI.Shared;

namespace UI.Pages
{
    internal enum ViewModeEnum
    {
        Category,
        Day
    }

    public partial class MonthDetail : BasePage
    {
        // ================================================================
        // 1. INYECCIONES
        // ================================================================

        [Inject]
        private HasDataService HasDataService { get; set; } = default!;

        // ================================================================
        // 2. PARÁMETROS
        // ================================================================

        [Parameter]
        public int Month { get; set; }

        [Parameter]
        public int Year { get; set; }

        // ================================================================
        // 3. MODELOS Y ESTADO
        // ================================================================

        private readonly CacheDictionary<int, AnnualDetailModel> _annualCache = new();

        private AnnualDetailModel? _annualData;
        private MonthDetailModel? _currentMonthData;
        private bool _isLoading = true;
        private Dictionary<string, bool> _expandedCategories = new();
        private Dictionary<int, bool> _expandedDays = new();
        private List<CategoryModel> _categories = new();

        private ViewModeEnum ViewMode = ViewModeEnum.Category;

        // Modal
        private bool _isTransactionModalOpen = false;
        private FormModeEnum _modalMode;
        private TransactionModel? _transactionToModal = null;

        // ================================================================
        // 4. CICLO DE VIDA
        // ================================================================

        protected override async Task OnParametersSetAsync()
        {
            if (Month == 0) Month = DateTime.Now.Month;
            if (Year == 0) Year = DateTime.Now.Year;

            await LoadCategories();
            await LoadDataAsync();
        }

        // ================================================================
        // 5. CARGA DE DATOS
        // ================================================================

        private async Task LoadCategories()
        {
            _categories = await APIService.GetCategoriesAsync() ?? new List<CategoryModel>();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                _isLoading = true;

                if (_annualCache.TryGetValue(Year, out List<AnnualDetailModel>? cachedList) &&
                    cachedList != null && cachedList.Count == 1)
                {
                    _annualData = cachedList[0];
                    _currentMonthData = _annualData.GetMonth(Month);
                    InitializeExpandedCategories();
                    InitializeExpandedDays();
                    StateHasChanged();
                    return;
                }

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

        // ================================================================
        // 6. INICIALIZACIÓN DE EXPANSIÓN
        // ================================================================

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
                _expandedDays[day] = true;
            }
        }

        // ================================================================
        // 7. MÉTODOS DE EXPANSIÓN (CATEGORÍAS Y DÍAS)
        // ================================================================

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
        // 8. VISTA POR DÍA
        // ================================================================

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

        // ================================================================
        // 9. NAVEGACIÓN ENTRE MESES
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
        // 10. MODAL DE TRANSACCIONES
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

            await HasDataService.RefreshAsync();

            _annualCache.Remove(Year);
            await LoadDataAsync();
            StateHasChanged();
        }

        private async Task OnTransactionModalCancelled()
        {
            _isTransactionModalOpen = false;
            _transactionToModal = null;
            StateHasChanged();
        }

        // ================================================================
        // 11. MÉTODOS AUXILIARES
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