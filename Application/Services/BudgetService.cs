using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    /// <summary>
    /// Implementación del servicio de presupuestos
    /// </summary>
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public BudgetService(IBudgetRepository budgetRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            ArgumentNullException.ThrowIfNull(budgetRepository);
            ArgumentNullException.ThrowIfNull(categoryRepository);
            ArgumentNullException.ThrowIfNull(mapper);

            _budgetRepository = budgetRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        // ==================== CONSULTAS ====================

        public async Task<BudgetResponseDTO> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null) throw new KeyNotFoundException($"Budget with ID {id} not found");

            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        public async Task<List<BudgetResponseDTO>> GetAllAsync()
        {
            IEnumerable<Budget> budgets = await _budgetRepository.GetAllAsync();

            return _mapper.Map<List<BudgetResponseDTO>>(budgets);
        }

        public async Task<List<BudgetResponseDTO>> GetByCategoryIdAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            IEnumerable<Budget> budgets = await _budgetRepository.GetByCategoryIdAsync(categoryId);
            return _mapper.Map<List<BudgetResponseDTO>>(budgets);
        }

        public async Task<List<BudgetResponseDTO>> GetByPeriodAsync(int month, int year)
        {
            MonthlyPeriod period = new MonthlyPeriod(month, year);
            IEnumerable<Budget> budgets = await _budgetRepository.GetByPeriodAsync(period);

            return _mapper.Map<List<BudgetResponseDTO>>(budgets);
        }

        public async Task<BudgetResponseDTO> GetByCategoryAndPeriodAsync(int categoryId, int month, int year)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            Budget budget = await _budgetRepository.GetByCategoryAndPeriodAsync(categoryId, period);

            if (budget == null) throw new KeyNotFoundException($"Budget not found for category {categoryId} in {month}/{year}");

            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        // ==================== RESUMEN (con cálculos) ====================

        public async Task<BudgetSummaryDTO> GetSummaryByCategoryAndPeriodAsync(int categoryId, int month, int year)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            Budget budget = await _budgetRepository.GetByCategoryAndPeriodAsync(categoryId, period);

            if (budget == null) throw new KeyNotFoundException($"Budget not found for category {categoryId} in {month}/{year}");

            // TODO: Obtener gastos reales (cuando esté implementado)
            // Por ahora, usamos 0 como total gastado
            decimal totalSpent = 0m;

            // Calcular estado del presupuesto
            BudgetStatusEnum status = budget.GetStatus(totalSpent);
            decimal percentageUsed = budget.GetPercentageUsed(totalSpent);
            Money remaining = budget.GetRemaining(totalSpent);
            bool isOverBudget = budget.IsOverBudget(totalSpent);

            return new BudgetSummaryDTO
            {
                BudgetId = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category?.Info?.Name ?? "Unknown",
                Year = year,
                Month = month,
                BudgetAmount = budget.MonthlyAmount.Value,
                TotalSpent = totalSpent,
                Remaining = remaining.Value,
                PercentageUsed = percentageUsed,
                Status = status,
                IsOverBudget = isOverBudget
            };
        }

        // ==================== COMANDOS ====================

        public async Task<BudgetResponseDTO> CreateAsync(CreateBudgetRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Validar que la categoría existe
            Category category = await _categoryRepository.GetByIdAsync(request.CategoryId, withTracking: true);

            if (category == null) throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            // Validar que no exista un presupuesto para la misma categoría y período
            MonthlyPeriod period = new MonthlyPeriod(request.Month, request.Year);
            bool exists = await _budgetRepository.ExistsForCategoryAndPeriodAsync(request.CategoryId, period);

            if (exists) throw new ConflictException($"Budget already exists for category {request.CategoryId} in {request.Month}/{request.Year}");

            // Crear entidad de dominio
            Money amount = new Money(request.Amount);
            Budget budget = new Budget(category, amount, period);

            // Guardar
            await _budgetRepository.AddAsync(budget);

            // Devolver DTO
            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        public async Task<BudgetResponseDTO> UpdateAsync(int id, UpdateBudgetRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null) throw new KeyNotFoundException($"Budget with ID {id} not found");

            // Actualizar importe
            Money newAmount = new Money(request.Amount);
            budget.UpdateAmount(newAmount);

            // Guardar
            await _budgetRepository.UpdateAsync(budget);

            // Devolver DTO
            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            if (!await _budgetRepository.ExistsAsync(id)) throw new KeyNotFoundException($"Budget with ID {id} not found");

            await _budgetRepository.DeleteAsync(id);
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0) return false;

            return await _budgetRepository.ExistsAsync(id);
        }

        public async Task<bool> ExistsForCategoryAndPeriodAsync(int categoryId, int month, int year)
        {
            if (categoryId <= 0) return false;

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            return await _budgetRepository.ExistsForCategoryAndPeriodAsync(categoryId, period);
        }
    }
}