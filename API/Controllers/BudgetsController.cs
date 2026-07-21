using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Request;
using Shared.DTOs.Response;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para gestionar presupuestos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        /// <summary>
        /// Constructor del controlador
        /// </summary>
        /// <param name="budgetService">Servicio de presupuestos</param>
        public BudgetController(IBudgetService budgetService)
        {
            ArgumentNullException.ThrowIfNull(budgetService);

            _budgetService = budgetService;
        }

        // ==================== CONSULTAS ====================

        /// <summary>
        /// Obtiene todos los presupuestos
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<BudgetResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            List<BudgetResponseDTO> budgets = await _budgetService.GetAllAsync();

            return Ok(budgets);
        }

        /// <summary>
        /// Obtiene un presupuesto por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BudgetResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid budget ID");

            BudgetResponseDTO budget = await _budgetService.GetByIdAsync(id);

            return Ok(budget);
        }

        /// <summary>
        /// Obtiene todos los presupuestos de una categoría
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        [ProducesResponseType(typeof(List<BudgetResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            List<BudgetResponseDTO> budgets = await _budgetService.GetByCategoryIdAsync(categoryId);

            return Ok(budgets);
        }

        /// <summary>
        /// Obtiene todos los presupuestos de un período (mes/año)
        /// </summary>
        [HttpGet("by-period")]
        [ProducesResponseType(typeof(List<BudgetResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByPeriod([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            List<BudgetResponseDTO> budgets = await _budgetService.GetByPeriodAsync(month, year);

            return Ok(budgets);
        }

        /// <summary>
        /// Obtiene un presupuesto por categoría y período
        /// </summary>
        [HttpGet("by-category-period")]
        [ProducesResponseType(typeof(BudgetResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByCategoryAndPeriod([FromQuery] int categoryId, [FromQuery] int year, [FromQuery] int month)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            BudgetResponseDTO budget = await _budgetService.GetByCategoryAndPeriodAsync(categoryId, month, year);

            return Ok(budget);
        }

        /// <summary>
        /// Obtiene un resumen completo del presupuesto (incluye estado y cálculos)
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(BudgetSummaryDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSummary([FromQuery] int categoryId, [FromQuery] int year, [FromQuery] int month)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            BudgetSummaryDTO summary = await _budgetService.GetSummaryByCategoryAndPeriodAsync(categoryId, month, year);

            return Ok(summary);
        }

        // ==================== COMANDOS ====================

        /// <summary>
        /// Crea un nuevo presupuesto
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BudgetResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateBudgetRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            BudgetResponseDTO createdBudget = await _budgetService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdBudget.Id }, createdBudget);
        }

        /// <summary>
        /// Crea un bloque de presupuestos por año
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(BulkBudgetResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateBulk([FromBody] CreateBulkBudgetRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            BulkBudgetResponseDTO createBulkBudget = await _budgetService.CreateBulkAsync(request);
            // 🔥 Usar el primer ID creado para la ubicación
            int firstId = createBulkBudget.AfectedIds.FirstOrDefault();
            if (firstId == 0) return BadRequest("No budgets were created.");

            return CreatedAtAction(nameof(GetById), new { id = new { id = firstId } }, createBulkBudget);
        }

        /// <summary>
        /// Actualiza un presupuesto existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(BudgetResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBudgetRequestDTO request)
        {
            if (id <= 0) return BadRequest("Invalid budget ID");

            if (request == null) return BadRequest("Request cannot be null");

            BudgetResponseDTO updatedBudget = await _budgetService.UpdateAsync(id, request);

            return Ok(updatedBudget);
        }

        /// <summary>
        /// Actualiza múltiples presupuestos en bloque
        /// </summary>
        [HttpPut("bulk")]
        [ProducesResponseType(typeof(BulkBudgetResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateBulk([FromBody] UpdateBulkBudgetRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            BulkBudgetResponseDTO result = await _budgetService.UpdateBulkAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Elimina un presupuesto
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Invalid budget ID");

            await _budgetService.DeleteAsync(id);

            return NoContent();
        }

        /// <summary>
        /// Elimina múltiples presupuestos en bloque
        /// </summary>
        [HttpDelete("bulk")]
        [ProducesResponseType(typeof(BulkBudgetResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteBulk([FromBody] DeleteBulkBudgetRequestDTO request)
        {
            if (request == null)
                return BadRequest("Request cannot be null");

            BulkBudgetResponseDTO result = await _budgetService.DeleteBulkAsync(request);
            return Ok(result);
        }
    }
}