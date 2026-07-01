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
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // ==================== CONSULTAS ====================

        public async Task<CategoryResponseDTO> GetByIdAsync(int id)
        {
            Category category = await _categoryRepository.GetByIdAsync(id);

            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found");

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<List<CategoryResponseDTO>> GetAllAsync()
        {
            List<Category> categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<List<CategoryResponseDTO>>(categories);
        }

        public async Task<List<CategoryResponseDTO>> GetActiveCategoriesAsync()
        {
            List<Category> categories = await _categoryRepository.GetActiveCategoriesAsync();
            return _mapper.Map<List<CategoryResponseDTO>>(categories);
        }

        public async Task<CategoryResponseDTO> GetByNameAsync(string name)
        {
            Category category = await _categoryRepository.GetByNameAsync(name);

            if (category == null) throw new KeyNotFoundException($"Category with name '{name}' not found");

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        // ==================== COMANDOS ====================

        public async Task<CategoryResponseDTO> CreateAsync(CreateCategoryRequestDTO request)
        {
            // Validar que no exista una categoría con el mismo nombre
            if (await _categoryRepository.ExistsByNameAsync(request.Name)) throw new InvalidOperationException($"Category with name '{request.Name}' already exists");

            // Crear entidad de dominio
            Category category = new Category(new EntityInfo(request.Name, request.Description));

            // Guardar
            await _categoryRepository.AddAsync(category);

            // Devolver DTO
            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO> UpdateAsync(int id, UpdateCategoryRequestDTO request)
        {
            // Obtener categoría existente
            Category category = await _categoryRepository.GetByIdAsync(id);

            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found");

            // Validar que el nuevo nombre no esté siendo usado por otra categoría
            Category existingCategory = await _categoryRepository.GetByNameAsync(request.Name);

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
            // Verificar si existe
            if (!await _categoryRepository.ExistsAsync(id)) throw new KeyNotFoundException($"Category with ID {id} not found");

            // Verificar si tiene gastos asociados
            if (await _categoryRepository.HasExpensesAsync(id)) throw new InvalidOperationException($"Category with ID {id} has associated expenses and cannot be deleted");

            // Eliminar
            await _categoryRepository.DeleteAsync(id);
        }

        // ==================== VALIDACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            return await _categoryRepository.ExistsAsync(id);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _categoryRepository.ExistsByNameAsync(name);
        }

        public async Task<bool> CanDeleteAsync(int id)
        {
            if (!await _categoryRepository.ExistsAsync(id)) return false;

            return !await _categoryRepository.HasExpensesAsync(id);
        }
    }
}