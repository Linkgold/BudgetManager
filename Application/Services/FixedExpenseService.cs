using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using Shared.DTOs.Request;
using Shared.DTOs.Response;

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

        public async Task<List<FixedExpenseResponseDTO>> GetAllByYearAsync(int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (year < 1900 || year > 2100) throw new ArgumentException("Year must be between 1900 and 2100", nameof(year));

            IEnumerable<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetAllByYearAsync(UserId, year);

            return _mapper.Map<List<FixedExpenseResponseDTO>>(fixedExpenses);
        }

        // ==================== COMANDOS ====================

        public async Task<FixedExpenseResponseDTO> CreateAsync(CreateFixedExpenseRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            // 🔥 Obtener el User completo
            User? user = await _userRepository.GetByIdAsync(UserId, withTracking: true);
            if (user == null) throw new KeyNotFoundException($"User with ID {UserId} not found");

            // ✅ Validar que la categoría existe
            Category? category = await _categoryRepository.GetByIdAsync(UserId, request.CategoryId, withTracking: true);
            if (category == null) throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            // ✅ Validar que no exista un gasto fijo con la misma combinación
            bool exists = await _fixedExpenseRepository.ExistsByCategoryNameMonthYearAsync(UserId, request.CategoryId, request.Name, request.Month, request.Year);
            if (exists)
            {
                throw new ConflictException($"Ya existe un gasto fijo con el nombre '{request.Name}' en la categoría seleccionada para {request.Month}/{request.Year}.");
            }

            // Crear Value Objects
            EntityInfo info = new EntityInfo(request.Name, request.Description);
            Money amount = new Money(request.Amount, request.Currency ?? "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(request.Month, request.Year);
            
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

            // ✅ Validar que no exista un gasto fijo con la misma combinación
            bool exists = await _fixedExpenseRepository.ExistsByCategoryNameMonthYearAsync(UserId, request.CategoryId, request.Name, request.Month, request.Year, id);
            if (exists)
            {
                throw new ConflictException($"Ya existe un gasto fijo con el nombre '{request.Name}' en la categoría seleccionada para {request.Month}/{request.Year}.");
            }

            // Obtener el gasto fijo existente
            FixedExpense? fixedExpense = await _fixedExpenseRepository.GetByIdAsync(UserId, id);
            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            // ✅ Validar que la categoría existe
            Category? category = await _categoryRepository.GetByIdAsync(UserId, request.CategoryId);
            if (category == null) throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            // Crear Value Objects
            EntityInfo info = new EntityInfo(request.Name, request.Description);
            Money amount = new Money(request.Amount, request.Currency ?? "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(request.Month, request.Year);

            // Actualizar entidad de dominio
            fixedExpense.Update(category, info, amount, chargePeriod);

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

        // ==================== VALIDACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) return false;

            return await _fixedExpenseRepository.ExistsAsync(UserId, id);
        }
    }
}