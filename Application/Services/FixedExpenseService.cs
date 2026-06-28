using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.ValueObjects;

namespace Application.Services
{
    /// <summary>
    /// Implementación del servicio de gastos fijos
    /// </summary>
    public class FixedExpenseService : IFixedExpenseService
    {
        private readonly IFixedExpenseRepository _fixedExpenseRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public FixedExpenseService(IFixedExpenseRepository fixedExpenseRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            ArgumentNullException.ThrowIfNull(fixedExpenseRepository);
            ArgumentNullException.ThrowIfNull(categoryRepository);
            ArgumentNullException.ThrowIfNull(mapper);

            _fixedExpenseRepository = fixedExpenseRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        // ==================== CONSULTAS ====================

        public async Task<FixedExpenseResponseDto> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            FixedExpense fixedExpense = await _fixedExpenseRepository.GetByIdAsync(id);

            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            return _mapper.Map<FixedExpenseResponseDto>(fixedExpense);
        }

        public async Task<List<FixedExpenseResponseDto>> GetAllAsync()
        {
            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetAllAsync();

            return _mapper.Map<List<FixedExpenseResponseDto>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDto>> GetByCategoryIdAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetByCategoryAsync(categoryId);

            return _mapper.Map<List<FixedExpenseResponseDto>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDto>> GetActiveAsync()
        {
            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetActiveAsync();

            return _mapper.Map<List<FixedExpenseResponseDto>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDto>> GetActiveByCategoryIdAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetActiveByCategoryAsync(categoryId);

            return _mapper.Map<List<FixedExpenseResponseDto>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDto>> GetActiveForPeriodAsync(int year, int month)
        {
            Period period = new Period(year, month);
            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetActiveForPeriodAsync(period);

            return _mapper.Map<List<FixedExpenseResponseDto>>(fixedExpenses);
        }

        public async Task<List<FixedExpenseResponseDto>> GetActiveForPeriodByCategoryAsync(int categoryId, int year, int month)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            Period period = new Period(year, month);
            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetActiveForPeriodByCategoryAsync(categoryId, period);

            return _mapper.Map<List<FixedExpenseResponseDto>>(fixedExpenses);
        }

        // ==================== COMANDOS ====================

        public async Task<FixedExpenseResponseDto> CreateAsync(CreateFixedExpenseRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Validar que la categoría existe
            Category category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null) throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            // Validar que no exista un gasto fijo con el mismo nombre en la misma categoría
            // Nota: Esto es opcional, pero ayuda a evitar duplicados
            // Podrías añadir un método en el repositorio para verificar por nombre y categoría

            // Crear Value Objects
            EntityInfo info = new EntityInfo(request.Name, request.Description);
            Money amount = new Money(request.Amount);
            Period chargePeriod = new Period(request.Year, request.Month);

            // Crear entidad de dominio
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Guardar
            await _fixedExpenseRepository.AddAsync(fixedExpense);

            // Devolver DTO
            return _mapper.Map<FixedExpenseResponseDto>(fixedExpense);
        }

        public async Task<FixedExpenseResponseDto> UpdateAsync(int id, UpdateFixedExpenseRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            // Obtener el gasto fijo existente
            FixedExpense fixedExpense = await _fixedExpenseRepository.GetByIdAsync(id);
            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            // Validar que el nuevo nombre no esté siendo usado por otro gasto fijo en la misma categoría
            // (opcional, depende de tus reglas de negocio)

            // Crear Value Objects
            EntityInfo info = new EntityInfo(request.Name, request.Description);
            Money amount = new Money(request.Amount);
            Period chargePeriod = new Period(request.Year, request.Month);

            // Actualizar entidad de dominio
            fixedExpense.Update(info, amount, chargePeriod);

            // Guardar
            await _fixedExpenseRepository.UpdateAsync(fixedExpense);

            // Devolver DTO
            return _mapper.Map<FixedExpenseResponseDto>(fixedExpense);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            if (!await _fixedExpenseRepository.ExistsAsync(id)) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            await _fixedExpenseRepository.DeleteAsync(id);
        }

        public async Task ActivateAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            if (!await _fixedExpenseRepository.ExistsAsync(id)) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            await _fixedExpenseRepository.ActivateAsync(id);
        }

        public async Task DeactivateAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            if (!await _fixedExpenseRepository.ExistsAsync(id)) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            await _fixedExpenseRepository.DeactivateAsync(id);
        }

        // ==================== VALIDACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0) return false;

            return await _fixedExpenseRepository.ExistsAsync(id);
        }

        public async Task<bool> IsActiveAsync(int id)
        {
            if (id <= 0) return false;

            if (!await _fixedExpenseRepository.ExistsAsync(id)) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            return await _fixedExpenseRepository.IsActiveAsync(id);
        }

        public async Task<decimal> GetTotalForPeriodByCategoryAsync(int categoryId, int year, int month)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(categoryId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            Period period = new Period(year, month);
            return await _fixedExpenseRepository.GetTotalByCategoryAndPeriodAsync(categoryId, period);
        }
    }
}