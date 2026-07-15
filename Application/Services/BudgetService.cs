using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Application.Services
{
    /// <summary>
    /// Implementación del servicio de presupuestos
    /// </summary>
    public class BudgetService : IBudgetService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public BudgetService
        (
            ICurrentUserService currentUserService,
            IBudgetRepository budgetRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IMapper mapper
        )
        {
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(budgetRepository);
            ArgumentNullException.ThrowIfNull(categoryRepository);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(mapper);

            _currentUserService = currentUserService;
            _budgetRepository = budgetRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        private int UserId => _currentUserService.UserId;

        // ==================== CONSULTAS ====================

        public async Task<BudgetResponseDTO> GetByIdAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget? budget = await _budgetRepository.GetByIdAsync(UserId, id);

            if (budget == null) throw new KeyNotFoundException($"Budget with ID {id} not found");

            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        public async Task<List<BudgetResponseDTO>> GetAllAsync()
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            IEnumerable<Budget> budgets = await _budgetRepository.GetAllAsync(UserId);

            return _mapper.Map<List<BudgetResponseDTO>>(budgets);
        }

        public async Task<List<BudgetResponseDTO>> GetByCategoryIdAsync(int categoryId)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(UserId, categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            IEnumerable<Budget> budgets = await _budgetRepository.GetByCategoryIdAsync(UserId, categoryId);
            return _mapper.Map<List<BudgetResponseDTO>>(budgets);
        }

        public async Task<List<BudgetResponseDTO>> GetByPeriodAsync(int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            IEnumerable<Budget> budgets = await _budgetRepository.GetByPeriodAsync(UserId, period);

            return _mapper.Map<List<BudgetResponseDTO>>(budgets);
        }

        public async Task<BudgetResponseDTO> GetByCategoryAndPeriodAsync(int categoryId, int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            Budget? budget = await _budgetRepository.GetByCategoryAndPeriodAsync(UserId, categoryId, period);

            if (budget == null) throw new KeyNotFoundException($"Budget not found for category {categoryId} in {month}/{year}");

            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        // ==================== RESUMEN (con cálculos) ====================

        public async Task<BudgetSummaryDTO> GetSummaryByCategoryAndPeriodAsync(int categoryId, int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            Budget? budget = await _budgetRepository.GetByCategoryAndPeriodAsync(UserId, categoryId, period);

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
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            ArgumentNullException.ThrowIfNull(request);

            // Validar que la categoría existe
            Category category = await _categoryRepository.GetByIdAsync(UserId, request.CategoryId, withTracking: true);

            if (category == null) throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            // Validar que no exista un presupuesto para la misma categoría y período
            MonthlyPeriod period = new MonthlyPeriod(request.Month, request.Year);
            bool exists = await _budgetRepository.ExistsForCategoryAndPeriodAsync(UserId, request.CategoryId, period);

            if (exists) throw new ConflictException($"Budget already exists for category {request.CategoryId} in {request.Month}/{request.Year}");

            // 🔥 Obtener el User completo
            User? user = await _userRepository.GetByIdAsync(UserId, withTracking: true);
            if (user == null) throw new KeyNotFoundException($"User with ID {UserId} not found");

            // Crear entidad de dominio
            Money amount = new Money(request.Amount);
            Budget budget = new Budget(user, category, amount, period);

            // Guardar
            await _budgetRepository.AddAsync(budget);

            // Devolver DTO
            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        public async Task<BudgetResponseDTO> UpdateAsync(int id, UpdateBudgetRequestDTO request)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            ArgumentNullException.ThrowIfNull(request);

            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget? budget = await _budgetRepository.GetByIdAsync(UserId, id);

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
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            if (!await _budgetRepository.ExistsAsync(UserId, id)) throw new KeyNotFoundException($"Budget with ID {id} not found");

            await _budgetRepository.DeleteAsync(UserId, id);
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            if (id <= 0) return false;

            return await _budgetRepository.ExistsAsync(UserId, id);
        }

        public async Task<bool> ExistsForCategoryAndPeriodAsync(int categoryId, int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) return false;

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            return await _budgetRepository.ExistsForCategoryAndPeriodAsync(UserId, categoryId, period);
        }
    }
}