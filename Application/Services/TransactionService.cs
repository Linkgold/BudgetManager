using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Application.Services
{
    /// <summary>
    /// Implementación del servicio de transacciones
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public TransactionService
        (
            ICurrentUserService currentUserService,
            ITransactionRepository transactionRepository, 
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IMapper mapper
        )
        {
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(transactionRepository);
            ArgumentNullException.ThrowIfNull(categoryRepository);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(mapper);

            _currentUserService = currentUserService;
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        private int UserId => _currentUserService.UserId;

        // ==================== CONSULTAS ====================

        public async Task<TransactionResponseDTO> GetByIdAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) throw new ArgumentException("Invalid transaction ID", nameof(id));

            Transaction transaction = await _transactionRepository.GetByIdAsync(id, UserId);

            if (transaction == null) throw new KeyNotFoundException($"Transaction with ID {id} not found");

            return _mapper.Map<TransactionResponseDTO>(transaction);
        }

        public async Task<List<TransactionResponseDTO>> GetAllAsync()
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            IEnumerable<Transaction> transactions = await _transactionRepository.GetAllAsync(UserId);

            return _mapper.Map<List<TransactionResponseDTO>>(transactions);
        }

        public async Task<List<TransactionResponseDTO>> GetByCategoryIdAsync(int categoryId)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(categoryId, UserId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            IEnumerable<Transaction> transactions = await _transactionRepository.GetByCategoryIdAsync(categoryId, UserId);

            return _mapper.Map<List<TransactionResponseDTO>>(transactions);
        }

        public async Task<List<TransactionResponseDTO>> GetByMonthlyPeriodAsync(int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            MonthlyPeriod period = new MonthlyPeriod(month, year);
            IEnumerable<Transaction> transactions = await _transactionRepository.GetByMonthlyPeriodAsync(period, UserId);

            return _mapper.Map<List<TransactionResponseDTO>>(transactions);
        }

        public async Task<List<TransactionResponseDTO>> GetByCategoryAndMonthlyPeriodAsync(int categoryId, int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(categoryId, UserId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            IEnumerable<Transaction> transactions = await _transactionRepository.GetByCategoryAndMonthlyPeriodAsync(categoryId, period, UserId);

            return _mapper.Map<List<TransactionResponseDTO>>(transactions);
        }

        public async Task<List<TransactionResponseDTO>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (from > to) throw new ArgumentException("Start date cannot be after end date", nameof(from));

            DailyPeriod startDate = new DailyPeriod(from.Day, from.Month, from.Year);
            DailyPeriod endDate = new DailyPeriod(to.Day, to.Month, to.Year);

            IEnumerable<Transaction> transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate, UserId);

            return _mapper.Map<List<TransactionResponseDTO>>(transactions);
        }

        public async Task<decimal> GetTotalByCategoryAndMonthlyPeriodAsync(int categoryId, int month, int year)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            if (!await _categoryRepository.ExistsAsync(categoryId, UserId)) throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            MonthlyPeriod period = new MonthlyPeriod(month, year);
            return await _transactionRepository.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, period, UserId);
        }

        // ==================== COMANDOS ====================

        public async Task<TransactionResponseDTO> CreateAsync(CreateTransactionRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Validar que la categoría existe
            Category category = await _categoryRepository.GetByIdAsync(request.CategoryId, UserId, withTracking: true);

            if (category == null) throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found");

            // Crear Value Objects
            EntityInfo info = new EntityInfo(request.Name, request.Description);
            Money amount = new Money(request.Amount);
            DailyPeriod date = new DailyPeriod(request.Date.Day, request.Date.Month, request.Date.Year);

            // 🔥 Obtener el User completo
            User? user = await _userRepository.GetByIdAsync(UserId, withTracking: true);
            if (user == null) throw new KeyNotFoundException($"User with ID {UserId} not found");

            // Crear entidad de dominio
            Transaction transaction = new Transaction(user, category, info, amount, request.Type, date);

            // Guardar
            await _transactionRepository.AddAsync(transaction);

            // Devolver DTO
            return _mapper.Map<TransactionResponseDTO>(transaction);
        }

        public async Task<TransactionResponseDTO> UpdateAsync(int id, UpdateTransactionRequestDTO request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (id <= 0) throw new ArgumentException("Invalid transaction ID", nameof(id));

            // Obtener la transacción existente
            Transaction? transaction = await _transactionRepository.GetByIdAsync(id, UserId);

            if (transaction == null) throw new KeyNotFoundException($"Transaction with ID {id} not found");

            // Crear Value Objects
            EntityInfo info = new EntityInfo(request.Name, request.Description);
            Money amount = new Money(request.Amount);
            DailyPeriod date = new DailyPeriod(request.Date.Day, request.Date.Month, request.Date.Year);

            // Actualizar entidad de dominio
            transaction.Update(info, amount, request.Type, date);

            // Guardar
            await _transactionRepository.UpdateAsync(transaction);

            // Devolver DTO
            return _mapper.Map<TransactionResponseDTO>(transaction);
        }

        public async Task DeleteAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) throw new ArgumentException("Invalid transaction ID", nameof(id));

            if (!await _transactionRepository.ExistsAsync(id, UserId)) throw new KeyNotFoundException($"Transaction with ID {id} not found");

            await _transactionRepository.DeleteAsync(id, UserId);
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (id <= 0) return false;

            return await _transactionRepository.ExistsAsync(id, UserId);
        }
    }
}