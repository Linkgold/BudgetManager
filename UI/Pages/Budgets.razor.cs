using Contracts.Enums;
using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using UI.Extensions;
using UI.Helpers;
using UI.Models;
using UI.Models.Cache;
using UI.Models.Forms;
using UI.Services;
using UI.Services.API;
using UI.Shared;

namespace UI.Pages
{
    public partial class Budgets : BasePage
    {
        // ================================================================
        // 1. INYECCIONES
        // ================================================================

        [Inject]
        private HasDataService HasDataService { get; set; } = default!;

        // ================================================================
        // 2. MODELOS Y ESTADO
        // ================================================================
        
        private readonly CacheDictionary<int, BudgetModel> _budgetsCache = new();
        private List<BudgetModel> _allBudgets = new();
        private List<CategoryModel> _allCategories = new();
        private List<CategoryModel> _filteredCategories = new();
        private List<CategoryModel> _availableCategoriesForCreate = new();
        private List<int> _years = new();
        private HasDataModel? _hasData;
        private bool _isLoading = true;

        private BudgetFormModel _budgetForm = new() { MonthlyAmounts = MonthHelper.Months.ToDictionary(m => m.Value, m => 0m) };

        // ================================================================
        // 3. FILTROS Y PROPIEDADES CON SETTER
        // ================================================================

        private string _searchTerm = string.Empty;
        private int _selectedYear = DateTime.Now.Year;
        private int _budgetYear;
        private int _selectedCategoryId = 0;
        private int _monthToDelete = 0;
        private bool _isConfirmModalOpenDeleteAll = false;
        private bool _isConfirmModalOpenDeleteOne = false;

        private bool HasCategories => _allCategories.Any();

        private string searchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    ApplyFilters();
                }
            }
        }
        
        private int selectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                }
            }
        }

        private int budgetYear
        {
            get => _budgetYear;
            set
            {
                if (_budgetYear != value)
                {
                    _budgetYear = value;
                    _budgetForm.Year = value; // Sincronizar con el modelo

                    // 🔥 EJECUTAR LÓGICA DE NEGOCIO sólo en el caso de creación (aquí tienes acceso a _allBudgets, _allCategories)
                    if (!_budgetForm.IsEditing && !_budgetForm.IsDeleting)
                    {
                        UpdateAvailableCategoriesForCreate();

                        if (_availableCategoriesForCreate.Any())
                        {
                            _budgetForm.CategoryId = _availableCategoriesForCreate.First().Id;
                        }
                        else
                        {
                            _budgetForm.CategoryId = 0;
                        }

                        StateHasChanged();
                    }
                }
            }
        }

        private int selectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                if (_selectedCategoryId != value)
                {
                    _selectedCategoryId = value;
                    _budgetForm.CategoryId = value;

                    // 🔥 Solo ejecutar lógica en modo creación
                    if (!_budgetForm.IsEditing && !_budgetForm.IsDeleting)
                    {
                        StateHasChanged();
                    }
                }
            }
        }

        private bool IsCurrentYearSelected => _selectedYear == DateTime.Now.Year;

        // ================================================================
        // 4. CICLO DE VIDA
        // ================================================================

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            _hasData = await HasDataService.GetDataAsync();
            _isLoading = false;
        }

        // ================================================================
        // 5. CARGA DE DATOS
        // ================================================================

        private async Task LoadData()
        {
            try
            {
                // Cargar categorías
                _allCategories = await APIService.GetCategoriesAsync() ?? new List<CategoryModel>();

                // Cargar presupuestos
                await LoadBudgets(_selectedYear);

                // Inicializar años (2020-2050)
                _years = new List<int>();
                for (int year = 2020; year <= 2050; year++)
                {
                    _years.Add(year);
                }

                // Inicializar formulario con los meses.
                _budgetForm.MonthlyAmounts = MonthHelper.GetMonthsWithShortName().ToDictionary(m => m.Value, m => 0m);
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar datos de presupuestos", ex);

                ToastService.ShowError("Error al cargar los datos de presupuestos.");

                _allCategories = new List<CategoryModel>();
                _allBudgets = new List<BudgetModel>();
            }
            finally
            {
                ApplyFilters();
            }
        }

        private async Task LoadBudgets(int year)
        {
            if (_budgetsCache.TryGetValue(year, out List<BudgetModel>? cached))
            {
                _allBudgets = cached!;
                return;
            }

            List<BudgetModel> budgets = await APIService.GetBudgetsAsync(year) ?? new List<BudgetModel>();

            _budgetsCache.Add(year, budgets);

            _allBudgets = budgets;
        }

        // ================================================================
        // 6. FILTRADO
        // ================================================================

        private void ApplyFilters()
        {
            // Filtrar presupuestos por año
            List<BudgetModel> budgetsForYear = _allBudgets.Where(b => b.Year == _selectedYear).ToList();

            // Obtener categorías con presupuestos para ese año
            List<int> categoryIds = budgetsForYear.Select(b => b.CategoryId).Distinct().ToList();

            // Obtener todas las categorías que tienen presupuestos en ese año
            _filteredCategories = _allCategories
                .Where(c => categoryIds.Contains(c.Id))
                .Where(c => string.IsNullOrEmpty(searchTerm) ||
                             c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderByNature(c => c.Nature)
                .ThenBy(c => c.Name)
                .ToList();
        }

        private void ClearSearch()
        {
            searchTerm = string.Empty;

            ApplyFilters();
        }

        private async Task SetCurrentYear()
        {
            _selectedYear = DateTime.Now.Year;

            await LoadBudgets(_selectedYear);

            ApplyFilters();

            await InvokeAsync(StateHasChanged);
        }

        private void UpdateAvailableCategoriesForCreate()
        {
            int currentYear = _budgetForm.Year > 0 ? _budgetForm.Year : selectedYear;

            // 🔥 Obtener IDs de categorías que YA TIENEN presupuesto para el año seleccionado en el formulario
            HashSet<int> categoriesWithBudget = _allBudgets
                .Where(b => b.Year == currentYear)
                .Select(b => b.CategoryId)
                .Distinct()
                .ToHashSet();

            // 🔥 Filtrar categorías: solo las que NO tienen presupuesto para ese año
            _availableCategoriesForCreate = _allCategories
                .Where(c => !categoriesWithBudget.Contains(c.Id))
                .OrderBy(c => c.Name)
                .ToList();

            // 🔥 Si la categoría seleccionada actualmente ya no está disponible, resetearla
            if (!_availableCategoriesForCreate.Any(c => c.Id == _budgetForm.CategoryId))
            {
                _budgetForm.CategoryId = _availableCategoriesForCreate.Any() ? _availableCategoriesForCreate.First().Id : 0;
            }
        }

        // ================================================================
        // 7. OPERACIONES CRUD (SAVE)
        // ================================================================

        private async Task SaveBudget()
        {
            try
            {
                bool success = false;

                if (_budgetForm.IsDeleting)
                {
                    // 🔥 Abrir confirmación antes de eliminar
                    OpenDeleteAllConfirmation();

                    return; // Salir del método, la confirmación continuará
                }
                else if (_budgetForm.IsEditing)
                {
                    success = await UpdateBulkBudgetAsync();
                }
                else
                {
                    success = await CreateBulkBudgetAsync();
                }

                if (success)
                {
                    _budgetsCache.Remove(_budgetForm.Year);

                    await HasDataService.RefreshAsync();

                    _budgetForm.IsModalOpen = false;
                    _budgetForm.IsEditing = false;
                    _budgetForm.IsDeleting = false;

                    await LoadBudgets(_selectedYear);

                    ApplyFilters();

                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveBudget", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task<bool> CreateBulkBudgetAsync()
        {
            List<KeyValuePair<int, decimal>> allMonths = _budgetForm.MonthlyAmounts.ToList();

            if (allMonths.Count == 0)
            {
                ToastService.ShowError("Debes asignar al menos un importe para crear un presupuesto.");
                return false;
            }

            CreateBulkBudgetRequestDTO request = new()
            {
                CategoryId = _budgetForm.CategoryId,
                Year = _budgetForm.Year,
                MonthlyBudgets = allMonths.Select
                (
                    kvp => new MonthlyBudgetDTO
                    {
                        Month = kvp.Key,
                        Amount = kvp.Value
                    }
                ).ToList()
            };

            BulkBudgetModel? result = await APIService.CreateBulkBudgetAsync(request);

            if (result != null)
            {
                ToastService.ShowSuccess($"Presupuestos creados correctamente para {_budgetForm.Year}.");
                return true;
            }

            return false;
        }

        private async Task<bool> UpdateBulkBudgetAsync()
        {
            List<KeyValuePair<int, decimal>> monthsToUpdate = _budgetForm.MonthlyAmounts.Where(kvp => kvp.Value > 0).ToList();

            if (!monthsToUpdate.Any())
            {
                ToastService.ShowError("Debes asignar al menos un importe para actualizar un presupuesto.");

                return false;
            }

            UpdateBulkBudgetRequestDTO request = new()
            {
                CategoryId = _budgetForm.CategoryId,
                Year = _budgetForm.Year,
                MonthlyBudgets = monthsToUpdate.Select
                (
                    kvp => new MonthlyBudgetDTO
                    {
                        Month = kvp.Key,
                        Amount = kvp.Value
                    }
                ).ToList()
            };

            BulkBudgetModel? result = await APIService.UpdateBulkBudgetAsync(request);

            if (result != null)
            {
                ToastService.ShowSuccess($"Presupuestos actualizados correctamente para {_budgetForm.Year}.");

                return true;
            }

            return false;
        }

        private async Task UpdateSingleMonthAsync(int month)
        {
            try
            {
                bool success = await UpdateMonthAmountAsync(month, _budgetForm.MonthlyAmounts[month]);

                if (success)
                {
                    ToastService.ShowSuccess($"Presupuesto de {month}/{_budgetForm.Year} actualizado correctamente.");
                    
                    // ✅ Actualizar el valor original para este mes
                    _budgetForm.OriginalMonthlyAmounts[month] = _budgetForm.MonthlyAmounts[month];

                    ApplyFilters();
                    StateHasChanged();
                }
                else
                {
                    ToastService.ShowError($"Error al actualizar el mes {month}.");
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en UpdateSingleMonth", ex);

                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task DeleteBulkBudgetAsync()
        {
            List<int> monthsWithBudget = _allBudgets
                .Where(b => b.CategoryId == _budgetForm.CategoryId && b.Year == _budgetForm.Year)
                .Select(b => b.Month)
                .Distinct()
                .ToList();

            if (monthsWithBudget.Count == 0)
            {
                ToastService.ShowError("No hay presupuestos para eliminar.");

                return;
            }

            DeleteBulkBudgetRequestDTO deleteRequest = new()
            {
                CategoryId = _budgetForm.CategoryId,
                Year = _budgetForm.Year,
                MonthsToDelete = monthsWithBudget
            };

            BulkBudgetModel? result = await APIService.DeleteBulkBudgetAsync(deleteRequest);

            if (result == null)
            {
                ToastService.ShowError("Error al eliminar los presupuestos.");

                return;
            }

            _budgetsCache.Remove(_budgetForm.Year);
            await HasDataService.RefreshAsync();
            ToastService.ShowSuccess($"Presupuestos de {_budgetForm.CategoryName} para {_budgetForm.Year} eliminados correctamente.");
        }

        private async Task DeleteSingleMonthAsync(int month)
        {
            try
            {
                bool success = await UpdateMonthAmountAsync(month, 0m);

                if (success)
                {
                    ToastService.ShowSuccess($"Presupuesto de {month}/{_budgetForm.Year} eliminado correctamente.");

                    ApplyFilters();
                    StateHasChanged();
                }
                else
                {
                    ToastService.ShowError($"Error al eliminar el mes {month}.");
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en DeleteSingleMonth", ex);

                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task<bool> UpdateMonthAmountAsync(int month, decimal newAmount)
        {
            BudgetModel? existingBudget = FindBudget(_budgetForm.CategoryId, month, _budgetForm.Year);

            if (existingBudget == null)
            {
                return false;
            }

            UpdateBudgetRequestDTO request = new() { Amount = newAmount };

            BudgetModel? result = await APIService.UpdateBudgetAsync(existingBudget.Id, request);
            if (result == null)
            {
                return false;
            }

            _budgetForm.MonthlyAmounts[month] = newAmount;
            existingBudget.Amount = newAmount;

            return true;
        }

        private BudgetModel? FindBudget(int categoryId, int month, int year)
        {
            return _allBudgets.FirstOrDefault(b => b.CategoryId == categoryId && b.Month == month && b.Year == year);
        }

        // ================================================================
        // 8. APERTURA DE MODALES
        // ================================================================

        private void OpenCreateModal()
        {
            UpdateAvailableCategoriesForCreate();

            FillFormFromModel
            (
                new BudgetModel
                {
                    Year = selectedYear,
                    CategoryId = _availableCategoriesForCreate.FirstOrDefault()?.Id ?? 0
                },
                FormModeEnum.Create
            );

            StateHasChanged();
        }

        private void OpenEditModal(int categoryId, int year)
        {
            BudgetModel? model = BuildBudgetModelFromExisting(categoryId, year);

            if (model != null)
            {
                FillFormFromModel(model, FormModeEnum.Edit);
                InvokeAsync(StateHasChanged);
            }

            StateHasChanged();
        }

        private void OpenDeleteModal(int categoryId, int year)
        {
            BudgetModel? model = BuildBudgetModelFromExisting(categoryId, year);

            if (model != null)
            {
                FillFormFromModel(model, FormModeEnum.Delete);
                StateHasChanged();
            }
        }

        private BudgetModel? BuildBudgetModelFromExisting(int categoryId, int year)
        {
            List<BudgetModel> existingBudgets = _allBudgets.Where(b => b.CategoryId == categoryId && b.Year == year).ToList();

            if (!existingBudgets.Any())
            {
                return null;
            }

            Dictionary<int, decimal> monthlyAmounts = MonthHelper.Months.ToDictionary
            (
                month => month.Value,
                month => existingBudgets.FirstOrDefault(b => b.Month == month.Value)?.Amount ?? 0
            );

            return new BudgetModel
            {
                CategoryId = categoryId,
                CategoryName = _allCategories.FirstOrDefault(c => c.Id == categoryId)?.Name ?? string.Empty,
                Year = year,
                Amount = monthlyAmounts.Values.FirstOrDefault(),
                Month = 0
            };
        }

        private void FillFormFromModel(BudgetModel model, FormModeEnum mode)
        {
            Dictionary<int, decimal> monthlyAmounts;

            if (mode == FormModeEnum.Create)
            {
                monthlyAmounts = MonthHelper.Months.ToDictionary(month => month.Value, month => 0m);
            }
            else
            {
                // Cargar los presupuestos existentes para esa categoría y año
                List<BudgetModel> existingBudgets = _allBudgets.Where(b => b.CategoryId == model.CategoryId && b.Year == model.Year).ToList();

                monthlyAmounts = MonthHelper.Months.ToDictionary
                (
                    month => month.Value,
                    month => existingBudgets.FirstOrDefault(b => b.Month == month.Value)?.Amount ?? 0
                );
            }

            _budgetForm = new BudgetFormModel
            {
                CategoryId = model.CategoryId,
                CategoryName = _allCategories.FirstOrDefault(c => c.Id == model.CategoryId)?.Name ?? string.Empty,
                Year = model.Year,
                DefaultAmount = monthlyAmounts.Values.FirstOrDefault(),
                MonthlyAmounts = monthlyAmounts,
                OriginalMonthlyAmounts = new Dictionary<int, decimal>(monthlyAmounts),
                IsModalOpen = true,
                IsEditing = mode == FormModeEnum.Edit,
                IsDeleting = mode == FormModeEnum.Delete
            };

            _budgetYear = model.Year;
            _selectedCategoryId = model.CategoryId;
        }

        private void CloseModal()
        {
            _budgetForm = new BudgetFormModel
            {
                CategoryId = 0,
                CategoryName = string.Empty,
                Year = 0,
                MonthlyAmounts = MonthHelper.Months.ToDictionary(month => month.Value, month => 0m),
                OriginalMonthlyAmounts = new Dictionary<int, decimal>(),
                IsModalOpen = false,
                IsEditing = false,
                IsDeleting = false,
                DefaultAmount = 0,
            };

            StateHasChanged();
        }

        // ================================================================
        // 9. CONFIRMACIONES DE ELIMINACIÓN
        // ================================================================

        private void OpenDeleteConfirmation(int month)
        {
            _monthToDelete = month;
            _isConfirmModalOpenDeleteOne = true;

            StateHasChanged();
        }

        private async Task ConfirmDeleteMonth()
        {
            await DeleteSingleMonthAsync(_monthToDelete);

            _isConfirmModalOpenDeleteOne = false;
            _monthToDelete = 0;

            StateHasChanged();
        }

        private void CancelDeleteMonth()
        {
            _monthToDelete = 0;
            _isConfirmModalOpenDeleteOne = false;

            StateHasChanged();
        }

        private void OpenDeleteAllConfirmation()
        {
            if (string.IsNullOrEmpty(_budgetForm.CategoryName))
            {
                ToastService.ShowError("No se puede eliminar: categoría no especificada.");

                return;
            }

            _isConfirmModalOpenDeleteAll = true;

            StateHasChanged();
        }

        private async Task ConfirmDeleteAll()
        {
            await DeleteBulkBudgetAsync();

            _budgetForm.IsModalOpen = false;
            _budgetForm.IsDeleting = false;
            _isConfirmModalOpenDeleteAll = false;

            await LoadBudgets(_selectedYear);

            ApplyFilters();
            StateHasChanged();
        }

        private void CancelDeleteAll()
        {
            _isConfirmModalOpenDeleteAll = false;

            StateHasChanged();
        }

        // ================================================================
        // 10. MÉTODOS AUXILIARES
        // ================================================================
        private decimal GetBudgetAmount(int categoryId, int month, int year)
        {
            BudgetModel? budget = FindBudget(categoryId, month, year);

            return budget?.Amount ?? 0;

            //return FindBudget(_budgetForm.CategoryId, month, _budgetForm.Year)?.Amount ?? 0;
        }

        private decimal GetCategoryTotal(int categoryId, int year)
        {
            CategoryModel? category = _allCategories.FirstOrDefault(c => c.Id == categoryId);
            if (category == null) return 0m;

            decimal total = _allBudgets
                .Where(b => b.CategoryId == categoryId && b.Year == year)
                .Sum(b => b.Amount);

            return Math.Abs(total);
        }

        private decimal GetMonthTotal(int month, int year)
        {
            decimal total = 0m;

            foreach (BudgetModel budget in _allBudgets.Where(b => b.Month == month && b.Year == year))
            {
                CategoryModel? category = _allCategories.FirstOrDefault(c => c.Id == budget.CategoryId);
                if (category == null) continue;

                if (category.Nature == CategoryNatureEnum.Income)
                {
                    total += budget.Amount;
                }
                else
                {
                    total -= budget.Amount;
                }
            }

            return total;
        }

        private decimal GetTotal(int year)
        {
            decimal total = 0m;

            foreach (BudgetModel budget in _allBudgets.Where(b => b.Year == year))
            {
                CategoryModel? category = _allCategories.FirstOrDefault(c => c.Id == budget.CategoryId);
                if (category == null) continue;

                if (category.Nature == CategoryNatureEnum.Income)
                {
                    total += budget.Amount;
                }
                else
                {
                    total -= budget.Amount;
                }
            }

            return total;
        }

        private void OnModalYearChanged(ChangeEventArgs e)
        {
            if (e.Value != null && int.TryParse(e.Value.ToString(), out int newYear))
            {
                budgetYear = newYear;
            }
        }

        private void OnCategoryChanged(ChangeEventArgs e)
        {
            if (e.Value != null && int.TryParse(e.Value.ToString(), out int newCategoryId))
            {
                selectedCategoryId = newCategoryId;

                if (_budgetForm.IsEditing)
                {
                    _budgetForm.OriginalMonthlyAmounts = new Dictionary<int, decimal>(_budgetForm.MonthlyAmounts);
                }

                StateHasChanged();
            }
        }

        private async Task OnYearChanged()
        {
            try
            {
                await LoadBudgets(_selectedYear);

                ApplyFilters();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync(
                    "Error al cargar los presupuestos del año seleccionado",
                    ex
                );

                ToastService.ShowError("Error al cargar los presupuestos.");

                _allBudgets = new List<BudgetModel>();

                ApplyFilters();
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}