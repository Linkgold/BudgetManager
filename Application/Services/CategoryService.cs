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
    /// Implementación del servicio de categorías
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CategoryService
        (
            ICurrentUserService currentUserService,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IMapper mapper
        )
        {
            ArgumentNullException.ThrowIfNull(currentUserService);
            ArgumentNullException.ThrowIfNull(categoryRepository);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(mapper);

            _currentUserService = currentUserService;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        private int UserId => _currentUserService.UserId;

        // ==================== CONSULTAS ====================

        public async Task<CategoryResponseDTO> GetByIdAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            Category? category = await _categoryRepository.GetByIdAsync(id, UserId);

            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found");

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            IEnumerable<Category> categories = await _categoryRepository.GetAllAsync(UserId);

            return _mapper.Map<List<CategoryResponseDTO>>(categories);
        }

        public async Task<List<CategoryResponseDTO>> GetActiveCategoriesAsync()
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            IEnumerable<Category> categories = await _categoryRepository.GetActiveCategoriesAsync(UserId);

            return _mapper.Map<List<CategoryResponseDTO>>(categories);
        }

        public async Task<CategoryResponseDTO> GetByNameAsync(string name)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            Category? category = await _categoryRepository.GetByNameAsync(name, UserId);

            if (category == null) throw new KeyNotFoundException($"Category with name '{name}' not found");

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        // ==================== COMANDOS ====================

        public async Task<CategoryResponseDTO> CreateAsync(CreateCategoryRequestDTO request)
        {
            // Validar que no exista una categoría con el mismo nombre
            if (await _categoryRepository.ExistsByNameAsync(request.Name, UserId)) throw new InvalidOperationException($"Category with name '{request.Name}' already exists");

            // 🔥 Obtener el User completo
            User? user = await _userRepository.GetByIdAsync(UserId, withTracking: true);
            if (user == null) throw new KeyNotFoundException($"User with ID {UserId} not found");

            // Crear entidad de dominio
            Category category = new Category(user, new EntityInfo(request.Name, request.Description));

            // Guardar
            await _categoryRepository.AddAsync(category);

            // Devolver DTO
            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO> UpdateAsync(int id, UpdateCategoryRequestDTO request)
        {
            // Obtener categoría existente
            Category? category = await _categoryRepository.GetByIdAsync(id, UserId);

            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found");

            // Validar que el nuevo nombre no esté siendo usado por otra categoría
            Category existingCategory = await _categoryRepository.GetByNameAsync(request.Name, UserId);

            if (existingCategory != null && existingCategory.Id != id) throw new InvalidOperationException($"Category with name '{request.Name}' already exists");

            // Actualizar entidad de dominio
            category.Update(new EntityInfo(request.Name, request.Description));

            // Guardar
            await _categoryRepository.UpdateAsync(category);

            // Devolver DTO
            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task DeleteAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            // Verificar si existe
            if (!await _categoryRepository.ExistsAsync(id, UserId)) throw new KeyNotFoundException($"Category with ID {id} not found");

            // Verificar  si tiene dependencias
            if (await _categoryRepository.HasDependenciesAsync(id, UserId)) throw new InvalidOperationException($"Category with ID {id} has associated expenses and cannot be deleted");

            // Eliminar
            await _categoryRepository.DeleteAsync(id, UserId);
        }

        // ==================== VALIDACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            return await _categoryRepository.ExistsAsync(id, UserId);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");

            return await _categoryRepository.ExistsByNameAsync(name, UserId);
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            if (UserId <= 0) throw new UnauthorizedAccessException("User is not authenticated");
            if (!await _categoryRepository.ExistsAsync(id, UserId)) return false;

            return !await _categoryRepository.HasDependenciesAsync(id, UserId);
        }
    }
}