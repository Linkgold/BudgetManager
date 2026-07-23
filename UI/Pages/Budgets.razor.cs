using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UI.Models;
using UI.Services.Interfaces;
using UI.Shared;

namespace UI.Pages
{
    public partial class Budgets: BasePage
    {
        private int _budgetYear;
        private int BudgetYear
        {
            get => _budgetYear;
            set
            {
                if (_budgetYear != value)
                {
                    _budgetYear = value;
                    budgetForm.Year = value; // Sincronizar con el modelo

                    // 🔥 EJECUTAR LÓGICA DE NEGOCIO sólo en el caso de creación (aquí tienes acceso a _allBudgets, _allCategories)
                    if (!budgetForm.IsEditing && !budgetForm.IsDeleting)
                    {
                        UpdateAvailableCategoriesForCreate();
                        budgetForm.MonthlyAmounts = _months.ToDictionary(month => month.Value, month => 0m);

                        if (_availableCategoriesForCreate.Any())
                        {
                            budgetForm.CategoryId = _availableCategoriesForCreate.First().Id;
                        }
                        else
                        {
                            budgetForm.CategoryId = 0;
                        }

                        StateHasChanged();
                    }
                }
            }
        }

        private int _selectedCategoryId;
        private int SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                if (_selectedCategoryId != value)
                {
                    _selectedCategoryId = value;
                    budgetForm.CategoryId = value;

                    // 🔥 Solo ejecutar lógica en modo creación
                    if (!budgetForm.IsEditing && !budgetForm.IsDeleting)
                    {
                        // Reiniciar meses al cambiar de categoría
                        budgetForm.MonthlyAmounts = _months.ToDictionary(month => month.Value, month => 0m);
                        StateHasChanged();
                    }
                }
            }
        }

        // ==================== DATOS ====================
        private List<CategoryResponseDTO> _allCategories = new();
        private List<CategoryResponseDTO> filteredCategories = new();
        private List<BudgetResponseDTO> _allBudgets = new();
        private List<CategoryResponseDTO> _availableCategoriesForCreate = new();

        private List<MonthModel> _months = new();
        private List<int> _years = new();

        private string _searchTerm = string.Empty;
        private int selectedYear = DateTime.Now.Year;

        private bool isModalOpen = false;
        private bool isEditing = false;
        private bool isDeleteMode = false;
        private BudgetFormModel budgetForm = new();

        private bool isConfirmModalOpenDeleteAll = false;
        private bool isConfirmModalOpenDeleteOne = false;
        private int _monthToDelete;

        [Inject]
        private IToastService ToastService { get; set; } = default!;

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

        protected override async Task OnInitializedAsync()
        {
            // 🔥 Datos de meses
            _months = new List<MonthModel>
            {
                new() { Value = 1, Name = "Enero", ShortName = "Ene" },
                new() { Value = 2, Name = "Febrero", ShortName = "Feb" },
                new() { Value = 3, Name = "Marzo", ShortName = "Mar" },
                new() { Value = 4, Name = "Abril", ShortName = "Abr" },
                new() { Value = 5, Name = "Mayo", ShortName = "May" },
                new() { Value = 6, Name = "Junio", ShortName = "Jun" },
                new() { Value = 7, Name = "Julio", ShortName = "Jul" },
                new() { Value = 8, Name = "Agosto", ShortName = "Ago" },
                new() { Value = 9, Name = "Septiembre", ShortName = "Sep" },
                new() { Value = 10, Name = "Octubre", ShortName = "Oct" },
                new() { Value = 11, Name = "Noviembre", ShortName = "Nov" },
                new() { Value = 12, Name = "Diciembre", ShortName = "Dic" }
            };

            // 🔥 Inicializar el diccionario del formulario con todos los meses
            budgetForm = new BudgetFormModel { MonthlyAmounts = _months.ToDictionary(m => m.Value, m => 0m) };

            // 🔥 Años (desde 2020 hasta 2030)
            _years = new List<int>();
            for (int year = 2020; year <= 2050; year++)
            {
                _years.Add(year);
            }

            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                // 🔥 Cargar categorías reales
                HttpResponseMessage categoriesResponse = await SendAuthenticatedRequestAsync(() => Http.GetAsync("/api/category"));
                if (categoriesResponse.IsSuccessStatusCode)
                {
                    List<CategoryResponseDTO>? categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryResponseDTO>>();
                    _allCategories = categories ?? new List<CategoryResponseDTO>();
                }
                else
                {
                    throw new InvalidOperationException($"Error al cargar categorías: {categoriesResponse.StatusCode}");
                }

                // 🔥 Cargar presupuestos reales para el año seleccionado
                HttpResponseMessage budgetsResponse = await SendAuthenticatedRequestAsync(() => Http.GetAsync("/api/budget"));
                if (budgetsResponse.IsSuccessStatusCode)
                {
                    List<BudgetResponseDTO>? budgets = await budgetsResponse.Content.ReadFromJsonAsync<List<BudgetResponseDTO>>();
                    _allBudgets = budgets ?? new List<BudgetResponseDTO>();
                }
                else
                {
                    throw new InvalidOperationException($"Error al cargar presupuestos: {budgetsResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar datos de presupuestos", ex);
                ToastService.ShowError("Error al cargar los datos de presupuestos.");
                _allCategories = new List<CategoryResponseDTO>();
                _allBudgets = new List<BudgetResponseDTO>();
            }
            finally
            {
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            // Filtrar presupuestos por año
            List<BudgetResponseDTO> budgetsForYear = _allBudgets
                .Where(b => b.Year == selectedYear)
                .ToList();

            // Obtener categorías con presupuestos para ese año
            List<int> categoryIds = budgetsForYear.Select(b => b.CategoryId).Distinct().ToList();

            // Obtener todas las categorías que tienen presupuestos en ese año
            filteredCategories = _allCategories
                .Where(c => categoryIds.Contains(c.Id))
                .Where(c => string.IsNullOrEmpty(searchTerm) ||
                             c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Name)
                .ToList();
        }

        // ==================== MÉTODOS DE CÁLCULO ====================

        private decimal GetBudgetAmount(int categoryId, int month, int year)
        {
            BudgetResponseDTO? budget = _allBudgets.FirstOrDefault(b => b.CategoryId == categoryId && b.Month == month && b.Year == year);

            return budget?.Amount ?? 0;
        }

        private decimal GetCategoryTotal(int categoryId, int year)
        {
            return _allBudgets
                .Where(b => b.CategoryId == categoryId && b.Year == year)
                .Sum(b => b.Amount);
        }

        private decimal GetMonthTotal(int month, int year)
        {
            return _allBudgets
                .Where(b => b.Month == month && b.Year == year)
                .Sum(b => b.Amount);
        }

        private decimal GetTotal(int year)
        {
            return _allBudgets
                .Where(b => b.Year == year)
                .Sum(b => b.Amount);
        }

        // ==================== MÉTODOS DEL MODAL ====================

        private void OpenCreateModal()
        {
            // 🔥 Inicializar el formulario con el año seleccionado
            budgetForm = new BudgetFormModel
            {
                CategoryId = 0,
                Year = selectedYear,
                MonthlyAmounts = _months.ToDictionary(month => month.Value, month => 0m),
                IsEditing = false,
                IsDeleting = false
            };

            // 🔥 Sincronizar la propiedad con el año del formulario
            BudgetYear = selectedYear;

            // 🔥 Actualizar las categorías disponibles para el año seleccionado
            UpdateAvailableCategoriesForCreate();

            // 🔥 Si hay categorías disponibles, seleccionar la primera
            if (_availableCategoriesForCreate.Any())
            {
                SelectedCategoryId = _availableCategoriesForCreate.First().Id;
            }
            else
            {
                SelectedCategoryId = 0;
            }

            isModalOpen = true;
            StateHasChanged();
        }

        private void OpenEditModal(int categoryId, int year)
        {
            // Cargar los presupuestos existentes para esa categoría y año
            List<BudgetResponseDTO> existingBudgets = _allBudgets
                .Where(budget => budget.CategoryId == categoryId && budget.Year == year)
                .ToList();

            if (!existingBudgets.Any())
                return;

            Dictionary<int, decimal> monthlyAmounts = _months.ToDictionary
            (
                month => month.Value,
                month => existingBudgets.FirstOrDefault(budget => budget.Month == month.Value)?.Amount ?? 0
            );

            budgetForm = new BudgetFormModel
            {
                CategoryId = categoryId,
                CategoryName = _allCategories.FirstOrDefault(c => c.Id == categoryId)?.Name ?? string.Empty,
                Year = year,
                DefaultAmount = GetDefaultAmount(monthlyAmounts),  // Asignamos el valor calculado
                MonthlyAmounts = monthlyAmounts,
                IsEditing = true,
                IsDeleting = false
            };

            // 🔥 Sincronizar la propiedad con el año y con la categoría
            BudgetYear = year;
            SelectedCategoryId = categoryId;

            isModalOpen = true;

            StateHasChanged();
        }

        private void OpenDeleteModal(int categoryId, int year)
        {
            // 🔥 Cargar los presupuestos existentes para esa categoría y año
            List<BudgetResponseDTO> existingBudgets = _allBudgets
                .Where(b => b.CategoryId == categoryId && b.Year == year)
                .ToList();

            if (!existingBudgets.Any())
                return;

            Dictionary<int, decimal> monthlyAmounts = _months.ToDictionary
            (
                month => month.Value,
                month => existingBudgets.FirstOrDefault(budget => budget.Month == month.Value)?.Amount ?? 0
            );

            budgetForm = new BudgetFormModel
            {
                CategoryId = categoryId,
                CategoryName = _allCategories.FirstOrDefault(c => c.Id == categoryId)?.Name ?? string.Empty,
                Year = year,
                DefaultAmount = GetDefaultAmount(monthlyAmounts),  // Asignamos el valor calculado
                MonthlyAmounts = monthlyAmounts,
                IsEditing = false,
                IsDeleting = true
            };

            // 🔥 Sincronizar la propiedad con el año y con la categoría
            BudgetYear = year;
            SelectedCategoryId = categoryId;

            isModalOpen = true;

            StateHasChanged();
        }

        private void ApplyAmountToAllMonths()
        {
            // Aplicar el importe del campo "budgetForm.Amount" a todos los meses
            foreach (MonthModel month in _months)
            {
                budgetForm.MonthlyAmounts[month.Value] = budgetForm.DefaultAmount;
            }
            StateHasChanged();
        }

        private decimal CalculateTotalAnnualBudget()
        {
            decimal total = 0m;

            foreach (KeyValuePair<int, decimal> kvp in budgetForm.MonthlyAmounts)
            {
                total += kvp.Value;
            }

            return total;
        }

        private async Task SaveBudget()
        {
            try
            {
                if (budgetForm.IsDeleting)
                {
                    // 🔥 Abrir confirmación antes de eliminar
                    OpenDeleteAllConfirmation();
                    return; // Salir del método, la confirmación continuará
                }
                else if (budgetForm.IsEditing)
                {
                    // 🔥 ========== MODO ACTUALIZACIÓN ========== //

                    // 1. Obtener los meses que tienen un importe > 0
                    List<KeyValuePair<int, decimal>> monthsToUpdate = budgetForm.MonthlyAmounts.Where(kvp => kvp.Value > 0).ToList();

                    if (!monthsToUpdate.Any())
                    {
                        ToastService.ShowError("Debes asignar al menos un importe para actualizar un presupuesto.");
                        return;
                    }

                    // 2. Crear el DTO para la actualización en bloque
                    UpdateBulkBudgetRequestDTO updateRequest = new UpdateBulkBudgetRequestDTO
                    {
                        CategoryId = budgetForm.CategoryId,
                        Year = budgetForm.Year,
                        MonthlyBudgets = monthsToUpdate.Select(kvp => new MonthlyBudgetDTO
                        {
                            Month = kvp.Key,
                            Amount = kvp.Value
                        }).ToList()
                    };

                    // 3. Llamar a la API para actualizar en bloque
                    HttpResponseMessage updateResponse = await SendAuthenticatedRequestAsync(() => Http.PutAsJsonAsync("/api/budget/bulk", updateRequest));

                    if (!updateResponse.IsSuccessStatusCode)
                    {
                        string errorContent = await updateResponse.Content.ReadAsStringAsync();
                        await LogService.LogErrorAsync($"Error al actualizar presupuestos en bloque", new Exception(errorContent));
                        ToastService.ShowError("Error al actualizar los presupuestos.");

                        return;
                    }

                    ToastService.ShowSuccess($"Presupuestos actualizados correctamente para {budgetForm.Year}.");
                }
                else
                {
                    // 1. Obtener los meses que tienen un importe > 0
                    List<KeyValuePair<int, decimal>> monthsToCreate = budgetForm.MonthlyAmounts.Where(kvp => kvp.Value > 0).ToList();

                    if (!monthsToCreate.Any())
                    {
                        ToastService.ShowError("Debes asignar al menos un importe para crear un presupuesto.");
                        return;
                    }

                    // 2. Crear el DTO para la llamada en bloque
                    CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
                    {
                        CategoryId = budgetForm.CategoryId,
                        Year = budgetForm.Year,
                        MonthlyBudgets = monthsToCreate.Select(kvp => new MonthlyBudgetDTO
                        {
                            Month = kvp.Key,
                            Amount = kvp.Value
                        }).ToList()
                    };

                    // 3. Llamar a la API para crear/actualizar en bloque
                    HttpResponseMessage response = await SendAuthenticatedRequestAsync(() => Http.PostAsJsonAsync("/api/budget/bulk", request));

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        await LogService.LogErrorAsync($"Error al crear/actualizar presupuestos en bloque", new Exception(errorContent));
                        ToastService.ShowError("Error al guardar los presupuestos.");
                        return;
                    }

                    ToastService.ShowSuccess($"Presupuestos creados/actualizados correctamente para {budgetForm.Year}.");
                }

                isModalOpen = false;

                await LoadData(); // Recargar datos actualizados

                ApplyFilters();

                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveBudget", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task UpdateSingleMonth(int month)
        {
            try
            {
                BudgetResponseDTO? existingBudget = _allBudgets
                    .FirstOrDefault(b => b.CategoryId == budgetForm.CategoryId &&
                                         b.Month == month &&
                                         b.Year == budgetForm.Year);

                if (existingBudget == null)
                {
                    ToastService.ShowError($"No hay presupuesto para {month}/{budgetForm.Year}");
                    return;
                }

                decimal newAmount = budgetForm.MonthlyAmounts[month];

                UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = newAmount };

                HttpResponseMessage response = await SendAuthenticatedRequestAsync(() => Http.PutAsJsonAsync($"/api/budget/{existingBudget.Id}", request));

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();

                    await LogService.LogErrorAsync($"Error al actualizar presupuesto ID {existingBudget.Id}", new Exception(errorContent));
                    ToastService.ShowError($"Error al actualizar el mes {month}.");

                    return;
                }

                // ✅ Actualizar lista local
                BudgetResponseDTO? localBudget = _allBudgets.FirstOrDefault(b => b.Id == existingBudget.Id);

                if (localBudget != null)
                {
                    localBudget.Amount = newAmount;
                }

                ToastService.ShowSuccess($"Presupuesto de {month}/{budgetForm.Year} actualizado correctamente.");

                ApplyFilters();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en UpdateSingleMonth", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private void CloseModal()
        {
            isModalOpen = false;
            isDeleteMode = false;
            StateHasChanged();
        }

        private string GetModalTitle()
        {
            if (budgetForm.IsDeleting)
            {
                return $"🗑️ Eliminar presupuestos de {budgetForm.CategoryName} ({budgetForm.Year})";
            }
            return isEditing ? "✏️ Editar Presupuesto" : "➕ Nuevo Presupuesto";
        }

        private string GetSaveButtonText()
        {
            return budgetForm.IsDeleting ? "Eliminar todos" : "Guardar";
        }

        private string GetSaveButtonClass()
        {
            return budgetForm.IsDeleting ? "btn-danger" : "btn-primary";
        }

        private decimal GetDefaultAmount(Dictionary<int, decimal> monthlyAmounts)
        {
            if (monthlyAmounts == null || monthlyAmounts.Count == 0) return 0m;

            decimal firstValue = monthlyAmounts.Values.First();
            bool allSame = monthlyAmounts.Values.All(v => v == firstValue);
            return allSame ? firstValue : 0m;
        }

        private void UpdateAvailableCategoriesForCreate()
        {
            // 🔥 Obtener IDs de categorías que YA TIENEN presupuesto para el año seleccionado en el formulario
            List<int> categoriesWithBudget = _allBudgets
                .Where(b => b.Year == budgetForm.Year)
                .Select(b => b.CategoryId)
                .Distinct()
                .ToList();

            // 🔥 Filtrar categorías: solo las que NO tienen presupuesto para ese año
            _availableCategoriesForCreate = _allCategories
                .Where(c => !categoriesWithBudget.Contains(c.Id))
                .ToList();

            // 🔥 Si la categoría seleccionada actualmente ya no está disponible, resetearla
            if (!_availableCategoriesForCreate.Any(c => c.Id == budgetForm.CategoryId))
            {
                budgetForm.CategoryId = _availableCategoriesForCreate.Any()
                    ? _availableCategoriesForCreate.First().Id
                    : 0;
            }
        }

        private void OnYearChanged(ChangeEventArgs e)
        {
            if (e.Value != null && int.TryParse(e.Value.ToString(), out int newYear))
            {
                BudgetYear = newYear;

                if (!budgetForm.IsEditing && !budgetForm.IsDeleting)
                {
                    UpdateAvailableCategoriesForCreate();
                    budgetForm.MonthlyAmounts = _months.ToDictionary(month => month.Value, month => 0m);

                    if (_availableCategoriesForCreate.Any())
                    {
                        budgetForm.CategoryId = _availableCategoriesForCreate.First().Id;
                    }
                    else
                    {
                        budgetForm.CategoryId = 0;
                    }

                    StateHasChanged();
                }
            }
        }

        private void OnCategoryChanged(ChangeEventArgs e)
        {
            if (e.Value != null && int.TryParse(e.Value.ToString(), out int newCategoryId))
            {
                SelectedCategoryId = newCategoryId;

                // 🔥 Reiniciar los meses cuando cambia la categoría
                budgetForm.MonthlyAmounts = _months.ToDictionary(month => month.Value, month => 0m);
                StateHasChanged();
            }
        }

        // ================ MÉTODOS DEL MODAL CONFIRMATION ===============

        private void OpenDeleteConfirmation(int month)
        {
            _monthToDelete = month;
            isConfirmModalOpenDeleteOne = true;

            StateHasChanged();
        }

        private async Task ConfirmDeleteMonth()
        {
            try
            {
                BudgetResponseDTO? existingBudget = _allBudgets
                    .FirstOrDefault(b => b.CategoryId == budgetForm.CategoryId &&
                                         b.Month == _monthToDelete &&
                                         b.Year == budgetForm.Year);

                if (existingBudget == null)
                {
                    ToastService.ShowError($"No hay presupuesto para {_monthToDelete}/{budgetForm.Year}");
                    return;
                }

                HttpResponseMessage response = await SendAuthenticatedRequestAsync(() => Http.DeleteAsync($"/api/budget/{existingBudget.Id}"));

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await LogService.LogErrorAsync($"Error al eliminar presupuesto ID {existingBudget.Id}", new Exception(errorContent));
                    ToastService.ShowError($"Error al eliminar el mes {_monthToDelete}.");
                    return;
                }

                // ✅ Actualizar lista local
                _allBudgets.RemoveAll(b => b.Id == existingBudget.Id);
                budgetForm.MonthlyAmounts[_monthToDelete] = 0;

                ToastService.ShowSuccess($"Presupuesto de {_monthToDelete}/{budgetForm.Year} eliminado correctamente.");

                ApplyFilters();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en ConfirmDeleteMonth", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private void CancelDeleteMonth()
        {
            _monthToDelete = 0;
            isConfirmModalOpenDeleteOne = false;
            StateHasChanged();
        }

        private void OpenDeleteAllConfirmation()
        {
            if (string.IsNullOrEmpty(budgetForm.CategoryName))
            {
                ToastService.ShowError("No se puede eliminar: categoría no especificada.");
                return;
            }

            isConfirmModalOpenDeleteAll = true;
            StateHasChanged();
        }

        private async Task ConfirmDeleteAll()
        {
            try
            {
                // 1. Obtener los meses que TIENEN PRESUPUESTO en la base de datos
                List<int> monthsWithBudget = _allBudgets
                    .Where(b => b.CategoryId == budgetForm.CategoryId && b.Year == budgetForm.Year)
                    .Select(b => b.Month)
                    .Distinct()
                    .ToList();

                if (monthsWithBudget.Count == 0)
                {
                    ToastService.ShowError("No hay presupuestos para eliminar.");
                    return;
                }

                // 2. Crear request de eliminación en bloque
                DeleteBulkBudgetRequestDTO deleteRequest = new DeleteBulkBudgetRequestDTO
                {
                    CategoryId = budgetForm.CategoryId,
                    Year = budgetForm.Year,
                    MonthsToDelete = monthsWithBudget
                };

                // 3. Llamar a la API
                HttpRequestMessage deleteHttpRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri("/api/budget/bulk", UriKind.Relative),
                    Content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json")
                };

                HttpResponseMessage deleteResponse = await SendAuthenticatedRequestAsync(() => Http.SendAsync(deleteHttpRequest));

                if (!deleteResponse.IsSuccessStatusCode)
                {
                    string errorContent = await deleteResponse.Content.ReadAsStringAsync();
                    await LogService.LogErrorAsync($"Error al eliminar presupuestos en bloque", new Exception(errorContent));
                    ToastService.ShowError("Error al eliminar los presupuestos.");
                    return;
                }

                ToastService.ShowSuccess($"Presupuestos de {budgetForm.CategoryName} para {budgetForm.Year} eliminados correctamente.");
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en ConfirmDeleteAll", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
            finally
            {
                isConfirmModalOpenDeleteAll = false;
                isModalOpen = false;
                await LoadData();
                ApplyFilters();
                StateHasChanged();
            }
        }

        private void CancelDeleteAll()
        {
            isConfirmModalOpenDeleteAll = false;
            StateHasChanged();
        }
    }
}