using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controlador para gestionar transacciones (gastos e ingresos)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
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
        [HttpGet("year/{year}")]
        [ProducesResponseType(typeof(List<TransactionResponseDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllByYear(int year)
        {
            List<TransactionResponseDTO> transactions = await _transactionService.GetAllByYearAsync(year);

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