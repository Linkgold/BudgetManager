using Application.DTOs;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FixedExpensesController : ControllerBase
    {
        private readonly FixedExpenseRepository _fixedExpenseRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly BudgetRepository _budgetRepository;

        public FixedExpensesController(
            FixedExpenseRepository fixedExpenseRepository,
            CategoryRepository categoryRepository,
            BudgetRepository budgetRepository)
        {
            _fixedExpenseRepository = fixedExpenseRepository;
            _categoryRepository = categoryRepository;
            _budgetRepository = budgetRepository;
        }

        /// <summary>
        /// Obtiene todos los gastos fijos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FixedExpenseDTO>>> GetAll()
        {
            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetAllAsync();
            List<FixedExpenseDTO> dtos = fixedExpenses.Select(fe => new FixedExpenseDTO
            {
                Id = fe.Id,
                Name = fe.Name,
                Amount = fe.Amount.Value,
                ChargeMonth = fe.ChargeMonth,
                Year = fe.Year,
                ChargeDay = fe.ChargeDay,
                Description = fe.Description,
                IsActive = fe.IsActive,
                CategoryId = fe.CategoryId,
                CategoryName = fe.Category?.Name,
                BudgetId = fe.BudgetId,
                CreatedAt = fe.CreatedAt,
                UpdatedAt = fe.UpdatedAt
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene un gasto fijo por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FixedExpenseDTO>> GetById(int id)
        {
            FixedExpense? fixedExpense = await _fixedExpenseRepository.GetByIdAsync(id);
            if (fixedExpense == null)
                return NotFound(new { message = "Gasto fijo no encontrado" });

            FixedExpenseDTO dto = new FixedExpenseDTO
            {
                Id = fixedExpense.Id,
                Name = fixedExpense.Name,
                Amount = fixedExpense.Amount.Value,
                ChargeMonth = fixedExpense.ChargeMonth,
                Year = fixedExpense.Year,
                ChargeDay = fixedExpense.ChargeDay,
                Description = fixedExpense.Description,
                IsActive = fixedExpense.IsActive,
                CategoryId = fixedExpense.CategoryId,
                CategoryName = fixedExpense.Category?.Name,
                BudgetId = fixedExpense.BudgetId,
                CreatedAt = fixedExpense.CreatedAt,
                UpdatedAt = fixedExpense.UpdatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo gasto fijo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FixedExpenseDTO>> Create([FromBody] CreateFixedExpenseDTO createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Category? category = null;
                if (createDto.CategoryId.HasValue)
                {
                    category = await _categoryRepository.GetByIdAsync(createDto.CategoryId.Value);
                    if (category == null)
                        return BadRequest(new { message = "Categoría no encontrada" });
                }

                FixedExpense fixedExpense = new FixedExpense(
                    createDto.Name,
                    new Money(createDto.Amount),
                    createDto.ChargeMonth,
                    createDto.Year,
                    createDto.ChargeDay,
                    createDto.Description,
                    category
                );

                // If BudgetId is provided, assign the budget
                if (createDto.BudgetId.HasValue)
                {
                    Budget? budget = await _budgetRepository.GetByIdAsync(createDto.BudgetId.Value);
                    if (budget == null)
                        return BadRequest(new { message = "Presupuesto no encontrado" });

                    fixedExpense.AssignBudget(budget);
                }

                await _fixedExpenseRepository.AddAsync(fixedExpense);
                await _fixedExpenseRepository.SaveChangesAsync();

                FixedExpenseDTO dto = new FixedExpenseDTO
                {
                    Id = fixedExpense.Id,
                    Name = fixedExpense.Name,
                    Amount = fixedExpense.Amount.Value,
                    ChargeMonth = fixedExpense.ChargeMonth,
                    Year = fixedExpense.Year,
                    ChargeDay = fixedExpense.ChargeDay,
                    Description = fixedExpense.Description,
                    IsActive = fixedExpense.IsActive,
                    CategoryId = fixedExpense.CategoryId,
                    CategoryName = fixedExpense.Category?.Name,
                    BudgetId = fixedExpense.BudgetId,
                    CreatedAt = fixedExpense.CreatedAt,
                    UpdatedAt = fixedExpense.UpdatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = fixedExpense.Id }, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el gasto fijo", detail = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un gasto fijo existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFixedExpenseDTO updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fixedExpense = await _fixedExpenseRepository.GetByIdAsync(id);
            if (fixedExpense == null)
                return NotFound(new { message = "Gasto fijo no encontrado" });

            try
            {
                Category? category = null;
                if (updateDto.CategoryId.HasValue)
                {
                    category = await _categoryRepository.GetByIdAsync(updateDto.CategoryId.Value);
                    if (category == null)
                        return BadRequest(new { message = "Categoría no encontrada" });

                    fixedExpense.AssignCategory(category);
                }

                fixedExpense.UpdateAmount(new Money(updateDto.Amount));

                _fixedExpenseRepository.Update(fixedExpense);
                await _fixedExpenseRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el gasto fijo", detail = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva un gasto fijo
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            FixedExpense? fixedExpense = await _fixedExpenseRepository.GetByIdAsync(id);
            if (fixedExpense == null)
                return NotFound(new { message = "Gasto fijo no encontrado" });

            try
            {
                fixedExpense.Deactivate();
                _fixedExpenseRepository.Update(fixedExpense);
                await _fixedExpenseRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al desactivar el gasto fijo", detail = ex.Message });
            }
        }

        /// <summary>
        /// Activa un gasto fijo
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            FixedExpense? fixedExpense = await _fixedExpenseRepository.GetByIdAsync(id);
            if (fixedExpense == null)
                return NotFound(new { message = "Gasto fijo no encontrado" });

            try
            {
                fixedExpense.Activate();
                _fixedExpenseRepository.Update(fixedExpense);
                await _fixedExpenseRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al activar el gasto fijo", detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene gastos fijos por categoría
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<FixedExpenseDTO>>> GetByCategory(int categoryId)
        {
            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetAllAsync();
            IEnumerable<FixedExpense> categoryExpenses = fixedExpenses.Where(fe => fe.CategoryId == categoryId);

            List<FixedExpenseDTO> dtos = categoryExpenses.Select(fe => new FixedExpenseDTO
            {
                Id = fe.Id,
                Name = fe.Name,
                Amount = fe.Amount.Value,
                ChargeMonth = fe.ChargeMonth,
                Year = fe.Year,
                ChargeDay = fe.ChargeDay,
                Description = fe.Description,
                IsActive = fe.IsActive,
                CategoryId = fe.CategoryId,
                CategoryName = fe.Category?.Name,
                BudgetId = fe.BudgetId,
                CreatedAt = fe.CreatedAt,
                UpdatedAt = fe.UpdatedAt
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene gastos fijos activos
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<FixedExpenseDTO>>> GetActive()
        {
            List<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetAllAsync();
            IEnumerable<FixedExpense> activeExpenses = fixedExpenses.Where(fe => fe.IsActive);

            List<FixedExpenseDTO> dtos = activeExpenses.Select(fe => new FixedExpenseDTO
            {
                Id = fe.Id,
                Name = fe.Name,
                Amount = fe.Amount.Value,
                ChargeMonth = fe.ChargeMonth,
                Year = fe.Year,
                ChargeDay = fe.ChargeDay,
                Description = fe.Description,
                IsActive = fe.IsActive,
                CategoryId = fe.CategoryId,
                CategoryName = fe.Category?.Name,
                BudgetId = fe.BudgetId,
                CreatedAt = fe.CreatedAt,
                UpdatedAt = fe.UpdatedAt
            }).ToList();

            return Ok(dtos);
        }
    }
}
