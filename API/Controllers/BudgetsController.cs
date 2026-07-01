using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para gestionar presupuestos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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

            List<BudgetResponseDTO> budgets = await _budgetService.GetByPeriodAsync(month,year);

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

            BudgetSummaryDTO summary = await _budgetService.GetSummaryByCategoryAndPeriodAsync(categoryId, month,year);

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
        /// Verifica si un presupuesto existe
        /// </summary>
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Exists(int id)
        {
            if (id <= 0) return BadRequest("Invalid budget ID");

            bool exists = await _budgetService.ExistsAsync(id);

            return Ok(new { Exists = exists });
        }

        /// <summary>
        /// Verifica si existe un presupuesto para una categoría y período
        /// </summary>
        [HttpGet("exists-by-category-period")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExistsForCategoryAndPeriod([FromQuery] int categoryId, [FromQuery] int year, [FromQuery] int month)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            bool exists = await _budgetService.ExistsForCategoryAndPeriodAsync(categoryId, month, year);

            return Ok(new { CategoryId = categoryId, Month=month, Year = year, Exists = exists });
        }
    }
}