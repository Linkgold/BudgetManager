using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Contracts.Enums;
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
        private readonly ITransactionManager _transactionManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public BudgetService
        (
            ITransactionManager transactionManager,
            ICurrentUserService currentUserService,
            IBudgetRepository budgetRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IMapper mapper
        )
        {
            ArgumentNullException.ThrowIfNull(transactionManager);
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(budgetRepository);
            ArgumentNullException.ThrowIfNull(categoryRepository);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(mapper);

            _transactionManager = transactionManager;
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

        /// <summary>
        /// Crea múltiples presupuestos para una categoría y año
        /// </summary>
        public async Task<BulkBudgetResponseDTO> CreateBulkAsync(CreateBulkBudgetRequestDTO request)
        {
            // Validar que el usuario está autenticado
            User? user = await GetAndValidateUserAuthenticatedAsync();

            ArgumentNullException.ThrowIfNull(request);

            // Validar que la categoría existe
            Category? category = await GatAndValidateCategoryExistsAsync(request.CategoryId);

            // Validar que hay meses con importe > 0
            List<MonthlyBudgetDTO> validMonths = request.MonthlyBudgets.Where(m => m.Amount > 0).ToList();

            if (validMonths.Count == 0) throw new ArgumentException("At least one month with amount > 0 is required");

            // Validar que los meses son válidos (1-12)
            foreach (MonthlyBudgetDTO month in validMonths)
            {
                if (month.Month < 1 || month.Month > 12) throw new ArgumentException($"Invalid month: {month.Month}");
            }

            await _transactionManager.BeginTransactionAsync();

            try
            {
                // 🔥 Crear los presupuestos en bloque
                List<int> createdIds = new List<int>();

                foreach (MonthlyBudgetDTO month in validMonths)
                {
                    MonthlyPeriod period = new MonthlyPeriod(month.Month, request.Year);
                    bool exists = await _budgetRepository.ExistsForCategoryAndPeriodAsync(UserId, request.CategoryId, period);

                    if (!exists)
                    {
                        Money money = new Money(month.Amount);
                        Budget budget = new Budget(user, category, money, period);
                        await _budgetRepository.AddAsync(budget);
                        createdIds.Add(budget.Id);
                    }
                }

                await _transactionManager.CommitTransactionAsync();

                // Devolver respuesta
                return new BulkBudgetResponseDTO
                {
                    CategoryId = request.CategoryId,
                    Year = request.Year,
                    CreatedIds = createdIds,
                    TotalCreated = createdIds.Count
                };
            }
            catch (Exception)
            {
                await _transactionManager.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<BudgetResponseDTO> CreateAsync(CreateBudgetRequestDTO request)
        {
            // 🔥 Obtener y validar el User completo
            User? user = await GetAndValidateUserAuthenticatedAsync();

            ArgumentNullException.ThrowIfNull(request);

            // Validar que la categoría existe
            Category? category = await GatAndValidateCategoryExistsAsync(request.CategoryId);

            // Validar que no exista un presupuesto para la misma categoría y período
            MonthlyPeriod period = new MonthlyPeriod(request.Month, request.Year);
            bool exists = await _budgetRepository.ExistsForCategoryAndPeriodAsync(UserId, request.CategoryId, period);

            if (exists) throw new ConflictException($"Budget already exists for category {request.CategoryId} in {request.Month}/{request.Year}");

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

        private async Task<User> GetAndValidateUserAuthenticatedAsync()
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            User? user = await _userRepository.GetByIdAsync(UserId, withTracking: true);
            if (user == null) throw new KeyNotFoundException($"User with ID {UserId} not found");

            return user;
        }

        private async Task<Category> GatAndValidateCategoryExistsAsync(int categoryId)
        {
            Category? category = await _categoryRepository.GetByIdAsync(UserId, categoryId, withTracking: true);
            if (category == null) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            return category;
        }
    }
}