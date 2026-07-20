using Microsoft.AspNetCore.Components;
using Shared.DTOs.Response;
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
        private BudgetResponseDTO budgetForm = new();

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
                new CategoryResponseDTO { Id = 4, Name = "Vivienda", Description = "Hogar y servicios" }
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
            isEditing = false;
            isDeleteMode = false;
            budgetForm = new BudgetResponseDTO { Month = DateTime.Now.Month, Year = selectedYear };
            isModalOpen = true;
            StateHasChanged();
        }

        private void OpenEditModal(int categoryId, int year)
        {
            // Buscar el primer presupuesto de esa categoría y año para editar
            BudgetResponseDTO? budget = _allBudgets.FirstOrDefault(b => b.CategoryId == categoryId && b.Year == year);

            if (budget != null)
            {
                isEditing = true;
                isDeleteMode = false;
                budgetForm = new BudgetResponseDTO
                {
                    Id = budget.Id,
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.CategoryName,
                    Month = budget.Month,
                    Year = budget.Year,
                    Amount = budget.Amount
                };
                isModalOpen = true;
                StateHasChanged();
            }
        }

        private async Task DeleteCategoryBudgets(int categoryId, int year)
        {
            // Eliminar todos los presupuestos de esa categoría para el año
            List<BudgetResponseDTO> budgetsToDelete = _allBudgets
                .Where(b => b.CategoryId == categoryId && b.Year == year)
                .ToList();

            if (budgetsToDelete.Any())
            {
                isDeleteMode = true;
                budgetForm = new BudgetResponseDTO
                {
                    CategoryId = categoryId,
                    CategoryName = _allCategories.FirstOrDefault(c => c.Id == categoryId)?.Name ?? string.Empty,
                    Year = year
                };
                isModalOpen = true;
                StateHasChanged();
            }
        }

        private async Task SaveBudget()
        {
            try
            {
                if (isDeleteMode)
                {
                    // Eliminar todos los presupuestos de la categoría y año
                    _allBudgets.RemoveAll(b => b.CategoryId == budgetForm.CategoryId && b.Year == budgetForm.Year);
                    ToastService.ShowSuccess($"Presupuestos de {budgetForm.CategoryName} para {budgetForm.Year} eliminados correctamente.");
                }
                else if (isEditing)
                {
                    // Actualizar el presupuesto
                    BudgetResponseDTO? existing = _allBudgets.FirstOrDefault(b => b.Id == budgetForm.Id);
                    if (existing != null)
                    {
                        existing.CategoryId = budgetForm.CategoryId;
                        existing.CategoryName = _allCategories.FirstOrDefault(c => c.Id == budgetForm.CategoryId)?.Name ?? string.Empty;
                        existing.Month = budgetForm.Month;
                        existing.Year = budgetForm.Year;
                        existing.Amount = budgetForm.Amount;
                    }
                    ToastService.ShowSuccess($"Presupuesto actualizado correctamente.");
                }
                else
                {
                    // Crear nuevo presupuesto
                    int newId = _allBudgets.Any() ? _allBudgets.Max(b => b.Id) + 1 : 1;
                    _allBudgets.Add(new BudgetResponseDTO
                    {
                        Id = newId,
                        CategoryId = budgetForm.CategoryId,
                        CategoryName = _allCategories.FirstOrDefault(c => c.Id == budgetForm.CategoryId)?.Name ?? string.Empty,
                        Month = budgetForm.Month,
                        Year = budgetForm.Year,
                        Amount = budgetForm.Amount
                    });
                    ToastService.ShowSuccess($"Presupuesto creado correctamente.");
                }

                isModalOpen = false;
                isDeleteMode = false;
                ApplyFilters();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveBudget", ex);
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
            if (isDeleteMode)
            {
                return $"🗑️ Eliminar presupuestos de {budgetForm.CategoryName} ({budgetForm.Year})";
            }
            return isEditing ? "✏️ Editar Presupuesto" : "➕ Nuevo Presupuesto";
        }

        private string GetSaveButtonText()
        {
            return isDeleteMode ? "Eliminar" : "Guardar";
        }

        private string GetSaveButtonClass()
        {
            return isDeleteMode ? "btn-danger" : "btn-primary";
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
