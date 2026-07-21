using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net.Http.Json;
using UI.Models;
using UI.Services.Interfaces;
using UI.Shared;

namespace UI.Pages
{
    public partial class Budgets: BasePage
    {
        // ==================== DATOS ====================
        private List<CategoryResponseDTO> _allCategories = new();
        private List<CategoryResponseDTO> filteredCategories = new();
        private List<BudgetResponseDTO> _allBudgets = new();

        private List<MonthFake> _months = new();
        private List<int> _years = new();

        private string _searchTerm = string.Empty;
        private int selectedYear = DateTime.Now.Year;

        private bool isModalOpen = false;
        private bool isEditing = false;
        private bool isDeleteMode = false;
        private BudgetFormModel budgetForm = new();

        private bool isConfirmModalOpen = false;
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
            // 🔥 Datos fake de meses
            _months = new List<MonthFake>
            {
                new MonthFake { Value = 1, Name = "Enero", ShortName = "Ene" },
                new MonthFake { Value = 2, Name = "Febrero", ShortName = "Feb" },
                new MonthFake { Value = 3, Name = "Marzo", ShortName = "Mar" },
                new MonthFake { Value = 4, Name = "Abril", ShortName = "Abr" },
                new MonthFake { Value = 5, Name = "Mayo", ShortName = "May" },
                new MonthFake { Value = 6, Name = "Junio", ShortName = "Jun" },
                new MonthFake { Value = 7, Name = "Julio", ShortName = "Jul" },
                new MonthFake { Value = 8, Name = "Agosto", ShortName = "Ago" },
                new MonthFake { Value = 9, Name = "Septiembre", ShortName = "Sep" },
                new MonthFake { Value = 10, Name = "Octubre", ShortName = "Oct" },
                new MonthFake { Value = 11, Name = "Noviembre", ShortName = "Nov" },
                new MonthFake { Value = 12, Name = "Diciembre", ShortName = "Dic" }
            };

            // 🔥 Años (desde 2020 hasta 2030)
            _years = new List<int>();
            for (int year = 2020; year <= 2030; year++)
            {
                _years.Add(year);
            }

            await LoadData();
        }

        private async Task LoadData()
        {
            // 🔥 Datos fake de categorías
            _allCategories = new List<CategoryResponseDTO>
            {
                new CategoryResponseDTO { Id = 1, Name = "Alimentación", Description = "Gastos de comida" },
                new CategoryResponseDTO { Id = 2, Name = "Transporte", Description = "Gastos de movilidad" },
                new CategoryResponseDTO { Id = 3, Name = "Ocio", Description = "Entretenimiento" },
                new CategoryResponseDTO { Id = 4, Name = "Vivienda 4", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 5, Name = "Vivienda 5", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 6, Name = "Vivienda 6", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 7, Name = "Vivienda 7", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 8, Name = "Vivienda 8", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 9, Name = "Vivienda 9", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 11, Name = "Vivienda 10", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 12, Name = "Vivienda 10", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 13, Name = "Vivienda 10", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 14, Name = "Vivienda 10", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 15, Name = "Vivienda 10", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 16, Name = "Vivienda 10", Description = "Hogar y servicios" },
                new CategoryResponseDTO { Id = 17, Name = "Vivienda 10", Description = "Hogar y servicios" },
            };

            // 🔥 Datos fake de presupuestos
            _allBudgets = new List<BudgetResponseDTO>
            {
                // Alimentación 2026
                new BudgetResponseDTO { Id = 1, CategoryId = 1, CategoryName = "Alimentación", Month = 1, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 2, CategoryId = 1, CategoryName = "Alimentación", Month = 2, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 3, CategoryId = 1, CategoryName = "Alimentación", Month = 3, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 4, CategoryId = 1, CategoryName = "Alimentación", Month = 4, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 5, CategoryId = 1, CategoryName = "Alimentación", Month = 5, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 6, CategoryId = 1, CategoryName = "Alimentación", Month = 6, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 7, CategoryId = 1, CategoryName = "Alimentación", Month = 7, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 8, CategoryId = 1, CategoryName = "Alimentación", Month = 8, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 9, CategoryId = 1, CategoryName = "Alimentación", Month = 9, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 10, CategoryId = 1, CategoryName = "Alimentación", Month = 10, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 11, CategoryId = 1, CategoryName = "Alimentación", Month = 11, Year = 2026, Amount = 500.00m },
                new BudgetResponseDTO { Id = 12, CategoryId = 1, CategoryName = "Alimentación", Month = 12, Year = 2026, Amount = 500.00m },
                
                // Transporte 2026
                new BudgetResponseDTO { Id = 13, CategoryId = 2, CategoryName = "Transporte", Month = 1, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 14, CategoryId = 2, CategoryName = "Transporte", Month = 2, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 15, CategoryId = 2, CategoryName = "Transporte", Month = 3, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 16, CategoryId = 2, CategoryName = "Transporte", Month = 4, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 17, CategoryId = 2, CategoryName = "Transporte", Month = 5, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 18, CategoryId = 2, CategoryName = "Transporte", Month = 6, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 19, CategoryId = 2, CategoryName = "Transporte", Month = 7, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 20, CategoryId = 2, CategoryName = "Transporte", Month = 8, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 21, CategoryId = 2, CategoryName = "Transporte", Month = 9, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 22, CategoryId = 2, CategoryName = "Transporte", Month = 10, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 23, CategoryId = 2, CategoryName = "Transporte", Month = 11, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 24, CategoryId = 2, CategoryName = "Transporte", Month = 12, Year = 2026, Amount = 200.00m },
                
                // Ocio 2026 (solo algunos meses)
                new BudgetResponseDTO { Id = 25, CategoryId = 3, CategoryName = "Ocio", Month = 6, Year = 2026, Amount = 150.00m },
                new BudgetResponseDTO { Id = 26, CategoryId = 3, CategoryName = "Ocio", Month = 7, Year = 2026, Amount = 200.00m },
                new BudgetResponseDTO { Id = 27, CategoryId = 3, CategoryName = "Ocio", Month = 8, Year = 2026, Amount = 180.00m },
            };

            int month = 1;
            int categoryId = 4;

            for (int i = 28; i <= 231; i++)
            {
                _allBudgets.Add(new BudgetResponseDTO { Id = i, CategoryId = categoryId, CategoryName = $"Vivienda {categoryId}", Month = month, Year = 2026, Amount = 150.00m });

                if (month < 13)
                {
                    month++;
                }
                else
                {
                    month = 1;
                    categoryId++;

                }
            }

            ApplyFilters();
            await Task.CompletedTask;
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
            budgetForm = new BudgetFormModel
            {
                CategoryId = 0,
                Year = selectedYear,
                MonthlyAmounts = _months.ToDictionary(month => month.Value, month => 0m),
                IsEditing = false,
                IsDeleting = false
            };

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

            isModalOpen = true;

            StateHasChanged();
        }

        private void ApplyAmountToAllMonths()
        {
            // Aplicar el importe del campo "budgetForm.Amount" a todos los meses
            foreach (MonthFake month in _months)
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
                if (isDeleteMode)
                {
                    // Eliminar todos los presupuestos de la categoría y año
                    _allBudgets.RemoveAll(budget => budget.CategoryId == budgetForm.CategoryId && budget.Year == budgetForm.Year);
                    ToastService.ShowSuccess($"Presupuestos de {budgetForm.CategoryName} para {budgetForm.Year} eliminados correctamente.");
                }
                else
                {
                    // 1. Obtener los meses que tienen un importe > 0
                    List<KeyValuePair<int, decimal>> monthsToCreate = budgetForm.MonthlyAmounts
                        .Where(kvp => kvp.Value > 0)
                        .ToList();

                    if (!monthsToCreate.Any())
                    {
                        ToastService.ShowError("Debes asignar al menos un importe para crear un presupuesto.");
                        return;
                    }

                    // 2. Eliminar presupuestos existentes para esa categoría y año (si es edición)
                    if (budgetForm.IsEditing)
                    {
                        _allBudgets.RemoveAll(budget => budget.CategoryId == budgetForm.CategoryId && budget.Year == budgetForm.Year);
                    }

                    // 3. Crear el DTO para la llamada en bloque
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

                    // 4. Llamar a la API
                    HttpResponseMessage response = await SendAuthenticatedRequestAsync(() => Http.PostAsJsonAsync("/api/budget/bulk", request));

                    // 5. Recargar datos y cerrar modal
                    ToastService.ShowSuccess($"Presupuestos creados/actualizados correctamente para {budgetForm.Year}.");
                }

                isModalOpen = false;
                
                await LoadData();
                
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
                // 1. Obtener el ID del presupuesto existente para ese mes
                BudgetResponseDTO? existingBudget = _allBudgets
                    .FirstOrDefault(b => b.CategoryId == budgetForm.CategoryId &&
                                         b.Month == month &&
                                         b.Year == budgetForm.Year);

                if (existingBudget == null)
                {
                    ToastService.ShowError($"No hay presupuesto para {month}/{budgetForm.Year}");
                    return;
                }

                // 2. Obtener el nuevo importe del diccionario
                decimal newAmount = budgetForm.MonthlyAmounts[month];

                // 3. Llamar a la API para actualizar
                UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = newAmount };
                HttpResponseMessage response = await SendAuthenticatedRequestAsync(() => Http.PutAsJsonAsync($"/api/budget/{existingBudget.Id}", request));

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    await LogService.LogErrorAsync($"Error al actualizar presupuesto ID {existingBudget.Id}", new Exception(errorContent));
                    ToastService.ShowError($"Error al actualizar el mes {month}.");
                    return;
                }

                // 4. Actualizar la lista local (para reflejar el cambio en la UI)
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
            return budgetForm.IsDeleting ? "Eliminar" : "Guardar";
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

        // ================ MÉTODOS DEL MODAL CONFIRMATION ===============

        private void OpenDeleteConfirmation(int month)
        {
            _monthToDelete = month;
            isConfirmModalOpen = true;

            StateHasChanged();
        }

        private async Task ConfirmDeleteMonth()
        {
            // 1. Obtener el ID del presupuesto existente para ese mes
            BudgetResponseDTO? existingBudget = _allBudgets
                .FirstOrDefault(b => b.CategoryId == budgetForm.CategoryId &&
                                     b.Month == _monthToDelete &&
                                     b.Year == budgetForm.Year);

            if (existingBudget == null)
            {
                ToastService.ShowError($"No hay presupuesto para {_monthToDelete}/{budgetForm.Year}");
                return;
            }

            // 2. Llamar a la API para eliminar
            HttpResponseMessage response = await SendAuthenticatedRequestAsync(() => Http.DeleteAsync($"/api/budget/{existingBudget.Id}"));

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                await LogService.LogErrorAsync($"Error al eliminar presupuesto ID {existingBudget.Id}", new Exception(errorContent));
                ToastService.ShowError($"Error al eliminar el mes {_monthToDelete}.");
                return;
            }

            // 3. Actualizar la lista local
            _allBudgets.RemoveAll(b => b.Id == existingBudget.Id);

            // 4. Actualizar el diccionario del modal
            budgetForm.MonthlyAmounts[_monthToDelete] = 0;

            ToastService.ShowSuccess($"Presupuesto de {_monthToDelete}/{budgetForm.Year} eliminado correctamente.");
            ApplyFilters();
            StateHasChanged();
        }

        private void CancelDeleteMonth()
        {
            _monthToDelete = 0;
            isConfirmModalOpen = false;
            StateHasChanged();
        }

        // ==================== CLASE FAKE PARA MESES ====================
        private class MonthFake
        {
            public int Value { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ShortName { get; set; } = string.Empty;
        }
    }
}
