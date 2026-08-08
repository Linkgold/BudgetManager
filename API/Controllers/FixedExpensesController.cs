using Shared.DTOs.Request;
using Shared.DTOs.Response;
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
        [HttpGet("year/{year}")]
        [ProducesResponseType(typeof(List<FixedExpenseResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllByYear(int year)
        {
            List<FixedExpenseResponseDTO> fixedExpenses = await _fixedExpenseService.GetAllByYearAsync(year);

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
    }
}