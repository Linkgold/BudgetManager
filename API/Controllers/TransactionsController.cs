using Application.DTOs;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para gestionar transacciones (gastos e ingresos)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            ArgumentNullException.ThrowIfNull(transactionService);
            _transactionService = transactionService;
        }

        // ==================== CONSULTAS ====================

        /// <summary>
        /// Obtiene todas las transacciones
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<TransactionResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            List<TransactionResponseDTO> transactions = await _transactionService.GetAllAsync();

            return Ok(transactions);
        }

        /// <summary>
        /// Obtiene una transacción por su ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TransactionResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid transaction ID");

            TransactionResponseDTO transaction = await _transactionService.GetByIdAsync(id);

            return Ok(transaction);
        }

        /// <summary>
        /// Obtiene todas las transacciones de una categoría
        /// </summary>
        [HttpGet("by-category/{categoryId}")]
        [ProducesResponseType(typeof(List<TransactionResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            List<TransactionResponseDTO> transactions = await _transactionService.GetByCategoryIdAsync(categoryId);

            return Ok(transactions);
        }

        /// <summary>
        /// Obtiene todas las transacciones de un período mensual (mes/año)
        /// </summary>
        [HttpGet("by-monthly-period")]
        [ProducesResponseType(typeof(List<TransactionResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByMonthlyPeriod([FromQuery] int month, [FromQuery] int year)
        {
            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            List<TransactionResponseDTO> transactions = await _transactionService.GetByMonthlyPeriodAsync(month, year);

            return Ok(transactions);
        }

        /// <summary>
        /// Obtiene todas las transacciones de una categoría y período mensual
        /// </summary>
        [HttpGet("by-category-monthly-period")]
        [ProducesResponseType(typeof(List<TransactionResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByCategoryAndMonthlyPeriod([FromQuery] int categoryId, [FromQuery] int month, [FromQuery] int year)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            List<TransactionResponseDTO> transactions = await _transactionService.GetByCategoryAndMonthlyPeriodAsync(categoryId, month, year);

            return Ok(transactions);
        }

        /// <summary>
        /// Obtiene todas las transacciones en un rango de fechas
        /// </summary>
        [HttpGet("by-date-range")]
        [ProducesResponseType(typeof(List<TransactionResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from > to) return BadRequest("Start date cannot be after end date");

            List<TransactionResponseDTO> transactions = await _transactionService.GetByDateRangeAsync(from, to);

            return Ok(transactions);
        }

        /// <summary>
        /// Obtiene el total de transacciones para una categoría y período mensual
        /// </summary>
        [HttpGet("total-by-category-monthly-period")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTotalByCategoryAndMonthlyPeriod([FromQuery] int categoryId, [FromQuery] int month, [FromQuery] int year)
        {
            if (categoryId <= 0) return BadRequest("Invalid category ID");

            if (month < 1 || month > 12) return BadRequest("Month must be between 1 and 12");

            if (year < 1900 || year > 2100) return BadRequest("Year must be between 1900 and 2100");

            decimal total = await _transactionService.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, month, year);

            return Ok(new { CategoryId = categoryId, Month = month, Year = year, Total = total });
        }

        // ==================== COMANDOS ====================

        /// <summary>
        /// Crea una nueva transacción
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TransactionResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequestDTO request)
        {
            if (request == null) return BadRequest("Request cannot be null");

            TransactionResponseDTO createdTransaction = await _transactionService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdTransaction.Id }, createdTransaction);
        }

        /// <summary>
        /// Actualiza una transacción existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TransactionResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTransactionRequestDTO request)
        {
            if (id <= 0) return BadRequest("Invalid transaction ID");

            if (request == null) return BadRequest("Request cannot be null");

            TransactionResponseDTO updatedTransaction = await _transactionService.UpdateAsync(id, request);

            return Ok(updatedTransaction);
        }

        /// <summary>
        /// Elimina una transacción
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Invalid transaction ID");

            await _transactionService.DeleteAsync(id);

            return NoContent();
        }
    }
}