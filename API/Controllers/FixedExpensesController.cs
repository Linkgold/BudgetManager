using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para gestionar gastos fijos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
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
        [HttpGet]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            List<FixedExpenseResponseDTO> fixedExpenses = await _fixedExpenseService.GetAllAsync();

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene un gasto fijo por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FixedExpenseResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            FixedExpenseResponseDTO fixedExpense = await _fixedExpenseService.GetByIdAsync(id);

            return Ok(fixedExpense);
        }

        /// <summary>
        /// Obtiene todos los gastos fijos de una categoría
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            List<FixedExpenseResponseDTO> fixedExpenses = await _fixedExpenseService.GetByCategoryIdAsync(categoryId);

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene todos los gastos fijos activos
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            List<FixedExpenseResponseDTO> fixedExpenses = await _fixedExpenseService.GetActiveAsync();

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene los gastos fijos activos para un período específico
        /// </summary>
        [HttpGet("active/period")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetActiveForPeriod([FromQuery] int month, [FromQuery] int year)
        {
            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            List<FixedExpenseResponseDTO> fixedExpenses = await _fixedExpenseService.GetActiveForPeriodAsync(month, year);

            return Ok(fixedExpenses);
        }

        /// <summary>
        /// Obtiene el total de gastos fijos para un período y categoría
        /// </summary>
        [HttpGet("total/period/category/{categoryId}")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTotalForPeriodByCategory(int categoryId, [FromQuery] int month, [FromQuery] int year)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            decimal total = await _fixedExpenseService.GetTotalForPeriodByCategoryAsync(categoryId, month, year);

            return Ok(new { CategoryId = categoryId, Year = year, Month = month, Total = total });
        }

        // ==================== COMANDOS ====================

        /// <summary>
        /// Crea un nuevo gasto fijo
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(FixedExpenseResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateFixedExpenseRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            FixedExpenseResponseDTO createdFixedExpense = await _fixedExpenseService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdFixedExpense.Id }, createdFixedExpense);
        }

        /// <summary>
        /// Actualiza un gasto fijo existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FixedExpenseResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFixedExpenseRequestDTO request)
        {
            if (id <= 0) return BadRequest("Invalid fixed expense ID");

            if (request == null) return BadRequest("Request cannot be null");

            FixedExpenseResponseDTO updatedFixedExpense = await _fixedExpenseService.UpdateAsync(id, request);

            return Ok(updatedFixedExpense);
        }

        /// <summary>
        /// Elimina un gasto fijo
        /// </summary>
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