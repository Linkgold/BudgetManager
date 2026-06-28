using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para gestionar gastos fijos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FixedExpenseController : ControllerBase
    {
        private readonly IFixedExpenseService _fixedExpenseService;

        /// <summary>
        /// Constructor del controlador
        /// </summary>
        /// <param name="fixedExpenseService">Servicio de gastos fijos</param>
        public FixedExpenseController(IFixedExpenseService fixedExpenseService)
        {
            ArgumentNullException.ThrowIfNull(fixedExpenseService);

            _fixedExpenseService = fixedExpenseService;
        }

        // ==================== CONSULTAS ====================

        /// <summary>
        /// Obtiene todos los gastos fijos
        /// </summary>
        /// <returns>Lista de gastos fijos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            List<FixedExpenseResponseDto> fixedExpenses = await _fixedExpenseService.GetAllAsync();

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene un gasto fijo por su ID
        /// </summary>
        /// <param name="id">ID del gasto fijo</param>
        /// <returns>Gasto fijo encontrado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FixedExpenseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            FixedExpenseResponseDto fixedExpense = await _fixedExpenseService.GetByIdAsync(id);

            return Ok(fixedExpense);
        }

        /// <summary>
        /// Obtiene todos los gastos fijos de una categoría
        /// </summary>
        /// <param name="categoryId">ID de la categoría</param>
        /// <returns>Lista de gastos fijos de la categoría</returns>
        [HttpGet("by-category/{categoryId}")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            List<FixedExpenseResponseDto> fixedExpenses = await _fixedExpenseService.GetByCategoryIdAsync(categoryId);

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene todos los gastos fijos activos
        /// </summary>
        /// <returns>Lista de gastos fijos activos</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            List<FixedExpenseResponseDto> fixedExpenses = await _fixedExpenseService.GetActiveAsync();

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene los gastos fijos activos de una categoría
        /// </summary>
        /// <param name="categoryId">ID de la categoría</param>
        /// <returns>Lista de gastos fijos activos de la categoría</returns>
        [HttpGet("active/by-category/{categoryId}")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActiveByCategory(int categoryId)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            List<FixedExpenseResponseDto> fixedExpenses = await _fixedExpenseService.GetActiveByCategoryIdAsync(categoryId);

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene los gastos fijos activos para un período específico
        /// </summary>
        /// <param name="year">Año</param>
        /// <param name="month">Mes (1-12)</param>
        /// <returns>Lista de gastos fijos activos para el período</returns>
        [HttpGet("active/period")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActiveForPeriod([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            List<FixedExpenseResponseDto> fixedExpenses = await _fixedExpenseService.GetActiveForPeriodAsync(year, month);

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene los gastos fijos activos para un período y categoría específicos
        /// </summary>
        /// <param name="categoryId">ID de la categoría</param>
        /// <param name="year">Año</param>
        /// <param name="month">Mes (1-12)</param>
        /// <returns>Lista de gastos fijos activos para el período y categoría</returns>
        [HttpGet("active/period/category/{categoryId}")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActiveForPeriodByCategory(int categoryId, [FromQuery] int year, [FromQuery] int month)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            List<FixedExpenseResponseDto> fixedExpenses = await _fixedExpenseService.GetActiveForPeriodByCategoryAsync(categoryId, year, month);

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene el total de gastos fijos para un período y categoría
        /// </summary>
        /// <param name="categoryId">ID de la categoría</param>
        /// <param name="year">Año</param>
        /// <param name="month">Mes (1-12)</param>
        /// <returns>Total de gastos fijos</returns>
        [HttpGet("total/period/category/{categoryId}")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTotalForPeriodByCategory(int categoryId, [FromQuery] int year, [FromQuery] int month)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            decimal total = await _fixedExpenseService.GetTotalForPeriodByCategoryAsync(categoryId, year, month);

            return Ok(new { CategoryId = categoryId, Year = year, Month = month, Total = total });
        }

        // ==================== COMANDOS ====================

        /// <summary>
        /// Crea un nuevo gasto fijo
        /// </summary>
        /// <param name="request">Datos del gasto fijo a crear</param>
        /// <returns>Gasto fijo creado</returns>
        [HttpPost]
        [ProducesResponseType(typeof(FixedExpenseResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateFixedExpenseRequestDto request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            FixedExpenseResponseDto createdFixedExpense = await _fixedExpenseService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdFixedExpense.Id }, createdFixedExpense);
        }

        /// <summary>
        /// Actualiza un gasto fijo existente
        /// </summary>
        /// <param name="id">ID del gasto fijo</param>
        /// <param name="request">Datos a actualizar</param>
        /// <returns>Gasto fijo actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FixedExpenseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFixedExpenseRequestDto request)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            if (request == null) return BadRequest("Request cannot be null");

            FixedExpenseResponseDto updatedFixedExpense = await _fixedExpenseService.UpdateAsync(id, request);

            return Ok(updatedFixedExpense);
        }

        /// <summary>
        /// Elimina un gasto fijo
        /// </summary>
        /// <param name="id">ID del gasto fijo</param>
        /// <returns>Sin contenido</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            await _fixedExpenseService.DeleteAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Activa un gasto fijo
        /// </summary>
        /// <param name="id">ID del gasto fijo</param>
        /// <returns>Sin contenido</returns>
        [HttpPatch("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Activate(int id)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            await _fixedExpenseService.ActivateAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Desactiva un gasto fijo
        /// </summary>
        /// <param name="id">ID del gasto fijo</param>
        /// <returns>Sin contenido</returns>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Deactivate(int id)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            await _fixedExpenseService.DeactivateAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Verifica si un gasto fijo existe
        /// </summary>
        /// <param name="id">ID del gasto fijo</param>
        /// <returns>True si existe, False si no</returns>
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Exists(int id)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            bool exists = await _fixedExpenseService.ExistsAsync(id);

            return Ok(new { Exists = exists });
        }

        /// <summary>
        /// Verifica si un gasto fijo está activo
        /// </summary>
        /// <param name="id">ID del gasto fijo</param>
        /// <returns>True si está activo, False si no</returns>
        [HttpGet("{id}/is-active")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> IsActive(int id)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            bool isActive = await _fixedExpenseService.IsActiveAsync(id);

            return Ok(new { Id = id, IsActive = isActive });
        }
    }
}