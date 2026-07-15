using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Application.Services
{
    /// <summary>
    /// Implementación del servicio de gastos fijos
    /// </summary>
    public class FixedExpenseService : IFixedExpenseService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IFixedExpenseRepository _fixedExpenseRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public FixedExpenseService
        (
            ICurrentUserService currentUserService,
            IFixedExpenseRepository fixedExpenseRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IMapper mapper
        )
        {
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(fixedExpenseRepository);
            ArgumentNullException.ThrowIfNull(categoryRepository);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(mapper);

            _currentUserService = currentUserService;
            _fixedExpenseRepository = fixedExpenseRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        private int UserId => _currentUserService.UserId;

        // ==================== CONSULTAS ====================

        public async Task<FixedExpenseResponseDTO> GetByIdAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            FixedExpense? fixedExpense = await _fixedExpenseRepository.GetByIdAsync(UserId, id);

            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            return _mapper.Map<FixedExpenseResponseDTO>(fixedExpense);
        }

        public async Task<List<FixedExpenseResponseDTO>> GetAllAsync()
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            IEnumerable<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetAllAsync(UserId);

            return _mapper.Map<List<FixedExpenseResponseDTO>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDTO>> GetByCategoryIdAsync(int categoryId)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(UserId, categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            IEnumerable<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetByCategoryAsync(UserId, categoryId);

            return _mapper.Map<List<FixedExpenseResponseDTO>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDTO>> GetActiveAsync()
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            IEnumerable<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetActiveAsync(UserId);

            return _mapper.Map<List<FixedExpenseResponseDTO>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDTO>> GetActiveByCategoryIdAsync(int categoryId)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(UserId, categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            IEnumerable<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetActiveByCategoryAsync(UserId, categoryId);

            return _mapper.Map<List<FixedExpenseResponseDTO>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDTO>> GetActiveForPeriodAsync(int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            MonthlyPeriod period = new MonthlyPeriod(month, year);
            IEnumerable<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetActiveForPeriodAsync(UserId, period);

            return _mapper.Map<List<FixedExpenseResponseDTO>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDTO>> GetActiveForPeriodByCategoryAsync(int categoryId, int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(UserId, categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            IEnumerable<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetActiveForPeriodByCategoryAsync(UserId, categoryId, period);

            return _mapper.Map<List<FixedExpenseResponseDTO>>(fixedExpenses);
        }

        // ==================== COMANDOS ====================

        public async Task<FixedExpenseResponseDTO> CreateAsync(CreateFixedExpenseRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Validar que la categoría existe
            Category? category = await _categoryRepository.GetByIdAsync(UserId, request.CategoryId, withTracking: true);
            if (category == null) throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            // Validar que no exista un gasto fijo con el mismo nombre en la misma categoría
            // Nota: Esto es opcional, pero ayuda a evitar duplicados
            // Podrías añadir un método en el repositorio para verificar por nombre y categoría

            // Crear Value Objects
            EntityInfo info = new EntityInfo(request.Name, request.Description);
            Money amount = new Money(request.Amount);
            MonthlyPeriod chargePeriod = new MonthlyPeriod(request.Month, request.Year);

            // 🔥 Obtener el User completo
            User? user = await _userRepository.GetByIdAsync(UserId, withTracking: true);
            if (user == null) throw new KeyNotFoundException($"User with ID {UserId} not found");

            // Crear entidad de dominio
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            // Guardar
            await _fixedExpenseRepository.AddAsync(fixedExpense);

            // Devolver DTO
            return _mapper.Map<FixedExpenseResponseDTO>(fixedExpense);
        }

        public async Task<FixedExpenseResponseDTO> UpdateAsync(int id, UpdateFixedExpenseRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            // Obtener el gasto fijo existente
            FixedExpense? fixedExpense = await _fixedExpenseRepository.GetByIdAsync(UserId, id);
            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            // Validar que el nuevo nombre no esté siendo usado por otro gasto fijo en la misma categoría
            // (opcional, depende de tus reglas de negocio)

            // Crear Value Objects
            EntityInfo info = new EntityInfo(request.Name, request.Description);
            Money amount = new Money(request.Amount);
            MonthlyPeriod chargePeriod = new MonthlyPeriod(request.Month, request.Year);

            // Actualizar entidad de dominio
            fixedExpense.Update(info, amount, chargePeriod);

            // Guardar
            await _fixedExpenseRepository.UpdateAsync(fixedExpense);

            // Devolver DTO
            return _mapper.Map<FixedExpenseResponseDTO>(fixedExpense);
        }

        public async Task DeleteAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            if (!await _fixedExpenseRepository.ExistsAsync(UserId, id)) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            await _fixedExpenseRepository.DeleteAsync(UserId, id);
        }

        public async Task ActivateAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            if (!await _fixedExpenseRepository.ExistsAsync(UserId, id)) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            await _fixedExpenseRepository.ActivateAsync(UserId, id);
        }

        public async Task DeactivateAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            if (!await _fixedExpenseRepository.ExistsAsync(UserId, id)) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            await _fixedExpenseRepository.DeactivateAsync(UserId, id);
        }

        // ==================== VALIDACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) return false;

            return await _fixedExpenseRepository.ExistsAsync(UserId, id);
        }

        public async Task<bool> IsActiveAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) return false;

            if (!await _fixedExpenseRepository.ExistsAsync(UserId, id)) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            return await _fixedExpenseRepository.IsActiveAsync(UserId, id);
        }

        public async Task<decimal> GetTotalForPeriodByCategoryAsync(int categoryId, int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(UserId, categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            return await _fixedExpenseRepository.GetTotalByCategoryAndPeriodAsync(UserId, categoryId, period);
        }
    }
}