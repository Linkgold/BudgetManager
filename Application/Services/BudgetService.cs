using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using Domain.Interfaces.Managers;

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

        public async Task<List<BudgetResponseDTO>> GetAllByYearAsync(int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (year < 1900 || year > 2100) throw new ArgumentException("Year must be between 1900 and 2100", nameof(year));

            IEnumerable<Budget> budgets = await _budgetRepository.GetAllByYearAsync(UserId, year);

            return _mapper.Map<List<BudgetResponseDTO>>(budgets);
        }

        // ==================== COMANDOS ====================

        /// <summary>
        /// Crea múltiples presupuestos para una categoría y año
        /// </summary>
        public async Task<BulkBudgetResponseDTO> CreateBulkAsync(CreateBulkBudgetRequestDTO request)
        {
            // 1. Validar que el usuario está autenticado
            User? user = await GetAndValidateUserAuthenticatedAsync();

            ArgumentNullException.ThrowIfNull(request);

            // 2. Validar que la categoría existe
            Category? category = await GatAndValidateCategoryExistsAsync(request.CategoryId);

            // 3. Validar que MonthlyBudgets no sea nulo y que tenga como mínimo un mese con importe > 0
            ValidateMonths(request.MonthlyBudgets);

            // 4. Transacción
            await _transactionManager.BeginTransactionAsync();

            try
            {
                // 🔥 Crear los presupuestos en bloque
                List<int> createdIds = new List<int>();

                foreach (MonthlyBudgetDTO month in request.MonthlyBudgets)
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
                    AfectedIds = createdIds,
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
            Money amount = new Money(request.Amount, request.Currency);
            Budget budget = new Budget(user, category, amount, period);

            // Guardar
            await _budgetRepository.AddAsync(budget);

            // Devolver DTO
            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        /// <summary>
        /// Actualiza múltiples presupuestos en bloque
        /// </summary>
        public async Task<BulkBudgetResponseDTO> UpdateBulkAsync(UpdateBulkBudgetRequestDTO request)
        {
            // 1. Validar que el usuario está autenticado
            User? user = await GetAndValidateUserAuthenticatedAsync();

            ArgumentNullException.ThrowIfNull(request);

            // 2. Validar que la categoría existe
            Category? category = await GatAndValidateCategoryExistsAsync(request.CategoryId);

            // 3. Validar que MonthlyBudgets no sea nulo y que tenga como mínimo un mese con importe > 0
            ValidateMonths(request.MonthlyBudgets);

            // 4. Transacción
            await _transactionManager.BeginTransactionAsync();

            try
            {
                List<int> updatedIds = new List<int>();

                foreach (MonthlyBudgetDTO month in request.MonthlyBudgets)
                {
                    MonthlyPeriod period = new MonthlyPeriod(month.Month, request.Year);
                    Budget? budget = await _budgetRepository.GetByCategoryAndPeriodAsync(UserId, request.CategoryId, period, withTracking: true);

                    if (budget != null)
                    {
                        // Actualizar existente
                        Money newAmount = new Money(month.Amount);
                        budget.Update(newAmount);
                        await _budgetRepository.UpdateAsync(budget);
                        updatedIds.Add(budget.Id);
                    }
                }

                await _transactionManager.CommitTransactionAsync();

                return new BulkBudgetResponseDTO
                {
                    CategoryId = request.CategoryId,
                    Year = request.Year,
                    AfectedIds = updatedIds,
                    TotalCreated = updatedIds.Count
                };
            }
            catch (Exception)
            {
                await _transactionManager.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<BudgetResponseDTO> UpdateAsync(int id, UpdateBudgetRequestDTO request)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            ArgumentNullException.ThrowIfNull(request);

            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget? budget = await _budgetRepository.GetByIdAsync(UserId, id);

            if (budget == null) throw new KeyNotFoundException($"Budget with ID {id} not found");

            // Actualizar importe
            budget.Update(amount: request.Amount, currency: request.Currency);

            // Guardar
            await _budgetRepository.UpdateAsync(budget);

            // Devolver DTO
            return _mapper.Map<BudgetResponseDTO>(budget);
        }

        /// <summary>
        /// Elimina múltiples presupuestos en bloque
        /// </summary>
        public async Task<BulkBudgetResponseDTO> DeleteBulkAsync(DeleteBulkBudgetRequestDTO request)
        {
            // 1. Validar usuario autenticado
            User? user = await GetAndValidateUserAuthenticatedAsync();

            ArgumentNullException.ThrowIfNull(request);

            // 2. Validar que la categoría existe
            Category? category = await GatAndValidateCategoryExistsAsync(request.CategoryId);

            // 3. Validar que hay meses a eliminar
            List<int> monthsToDelete = request.MonthsToDelete.Where(m => m >= 1 && m <= 12).Distinct().ToList();

            if (monthsToDelete.Count == 0) throw new ArgumentException("At least one month to delete is required");

            // 4. Transacción
            await _transactionManager.BeginTransactionAsync();

            try
            {
                List<int> deletedIds = new List<int>();

                foreach (int id in monthsToDelete)
                {
                    MonthlyPeriod period = new MonthlyPeriod(id, request.Year);
                    Budget? budget = await _budgetRepository.GetByCategoryAndPeriodAsync(UserId, request.CategoryId, period);

                    if (budget != null)
                    {
                        await _budgetRepository.DeleteAsync(UserId, budget.Id);
                        deletedIds.Add(budget.Id);
                    }
                }

                await _transactionManager.CommitTransactionAsync();

                return new BulkBudgetResponseDTO
                {
                    CategoryId = request.CategoryId,
                    Year = request.Year,
                    AfectedIds = deletedIds,
                    TotalCreated = deletedIds.Count
                };
            }
            catch (Exception)
            {
                await _transactionManager.RollbackTransactionAsync();
                throw;
            }
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

        private void ValidateMonths(List<MonthlyBudgetDTO> monthlyBudgets)
        {
            ArgumentNullException.ThrowIfNull(monthlyBudgets);

            if (!monthlyBudgets.Any(x => x.Amount > 0)) throw new ArgumentException("At least one month with amount > 0 is required");
        }
    }
}