using Application.DTOs;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoriesController(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Obtiene todas las categorías
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAll()
        {
            List<Category> categories = await _categoryRepository.GetAllAsync();
            List<CategoryDTO> dtos = categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene una categoría por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetById(int id)
        {
            Category? category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            CategoryDTO dto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Crea una nueva categoría
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategoryDTO>> Create([FromBody] CreateCategoryDTO createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Category category = new Category(createDto.Name, createDto.Description);
                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();

                CategoryDTO dto = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear la categoría", detail = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza una categoría existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDTO updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Category? category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            try
            {
                category.Update(updateDto.Name, updateDto.Description);
                _categoryRepository.Update(category);
                await _categoryRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la categoría", detail = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva una categoría
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Category? category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            try
            {
                category.Deactivate();
                _categoryRepository.Update(category);
                await _categoryRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al desactivar la categoría", detail = ex.Message });
            }
        }

        /// <summary>
        /// Activa una categoría
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            Category? category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Categoría no encontrada" });

            try
            {
                category.Activate();
                _categoryRepository.Update(category);
                await _categoryRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al activar la categoría", detail = ex.Message });
            }
        }
    }
}
