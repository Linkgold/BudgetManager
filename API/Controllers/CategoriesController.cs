using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para gestionar categorías
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        /// <summary>
        /// Constructor del controlador
        /// </summary>
        /// <param name="categoryService">Servicio de categorías</param>
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        /// <summary>
        /// Obtiene todas las categorías
        /// </summary>
        /// <returns>Lista de categorías</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            List<CategoryResponseDto> categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Obtiene una categoría por su ID
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Categoría encontrada</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid category ID");

            CategoryResponseDto category = await _categoryService.GetByIdAsync(id);
            return Ok(category);
        }

        /// <summary>
        /// Obtiene todas las categorías activas
        /// </summary>
        /// <returns>Lista de categorías activas</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            List<CategoryResponseDto> categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Obtiene una categoría por su nombre
        /// </summary>
        /// <param name="name">Nombre de la categoría</param>
        /// <returns>Categoría encontrada</returns>
        [HttpGet("byname/{name}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Category name cannot be empty");

            CategoryResponseDto category = await _categoryService.GetByNameAsync(name);
            return Ok(category);
        }

        /// <summary>
        /// Crea una nueva categoría
        /// </summary>
        /// <param name="request">Datos de la categoría a crear</param>
        /// <returns>Categoría creada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            CategoryResponseDto createdCategory = await _categoryService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }

        /// <summary>
        /// Actualiza una categoría existente
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Categoría actualizada</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequestDto request)
        {
            if (id <= 0) return BadRequest("Invalid category ID");

            if (request == null) return BadRequest("Request cannot be null");

            CategoryResponseDto updatedCategory = await _categoryService.UpdateAsync(id, request);
            return Ok(updatedCategory);
        }

        /// <summary>
        /// Elimina una categoría
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Sin contenido</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Invalid category ID");

            await _categoryService.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Verifica si una categoría existe
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>True si existe, False si no</returns>
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Exists(int id)
        {
            if (id <= 0) return BadRequest("Invalid category ID");

            bool exists = await _categoryService.ExistsAsync(id);
            return Ok(new { Exists = exists });
        }

        /// <summary>
        /// Verifica si una categoría puede ser eliminada
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>True si se puede eliminar, False si no</returns>
        [HttpGet("{id}/can-delete")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CanDelete(int id)
        {
            if (id <= 0) return BadRequest("Invalid category ID");

            bool canDelete = await _categoryService.CanDeleteAsync(id);
            return Ok(new { CanDelete = canDelete });
        }
    }
}