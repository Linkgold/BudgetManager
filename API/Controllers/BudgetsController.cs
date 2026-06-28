using Application.DTOs;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController : ControllerBase
    {
        private readonly BudgetRepository _budgetRepository;
        private readonly CategoryRepository _categoryRepository;

        public BudgetsController(BudgetRepository budgetRepository, CategoryRepository categoryRepository)
        {
            _budgetRepository = budgetRepository;
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Obtiene todos los presupuestos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetDTO>>> GetAll()
        {
            List<Budget> budgets = await _budgetRepository.GetAllAsync();
            List<BudgetDTO> dtos = budgets.Select(b => new BudgetDTO
            {
                Id = b.Id,
                MonthlyAmount = b.MonthlyAmount.Value,
                Month = b.Period.Month,
                Year = b.Period.Year,
                CategoryId = b.CategoryId,
                //CategoryName = b.Category?.Name,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene un presupuesto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetDTO>> GetById(int id)
        {
            Budget? budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null)
                return NotFound(new { message = "Presupuesto no encontrado" });

            BudgetDTO dto = new BudgetDTO
            {
                Id = budget.Id,
                MonthlyAmount = budget.MonthlyAmount.Value,
                Month = budget.Period.Month,
                Year = budget.Period.Year,
                CategoryId = budget.CategoryId,
                //CategoryName = budget.Category?.Name,
                CreatedAt = budget.CreatedAt,
                UpdatedAt = budget.UpdatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo presupuesto
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BudgetDTO>> Create([FromBody] CreateBudgetDTO createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Category? category = await _categoryRepository.GetByIdAsync(createDto.CategoryId);
                if (category == null)
                    return BadRequest(new { message = "Categoría no encontrada" });

                Budget budget = new Budget(
                    category,
                    new Domain.ValueObjects.Money(createDto.MonthlyAmount),
                    new Domain.ValueObjects.Period(createDto.Month, createDto.Year)
                );

                await _budgetRepository.AddAsync(budget);
                await _budgetRepository.SaveChangesAsync();

                BudgetDTO dto = new BudgetDTO
                {
                    Id = budget.Id,
                    MonthlyAmount = budget.MonthlyAmount.Value,
                    Month = budget.Period.Month,
                    Year = budget.Period.Year,
                    CategoryId = budget.CategoryId,
                    //CategoryName = budget.Category?.Name,
                    CreatedAt = budget.CreatedAt,
                    UpdatedAt = budget.UpdatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = budget.Id }, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el presupuesto", detail = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza un presupuesto existente (solo el monto mensual)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBudgetDTO updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Budget? budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null)
                return NotFound(new { message = "Presupuesto no encontrado" });

            try
            {
                // Update only the monthly amount as other properties are immutable after creation
                budget.UpdateAmount(new Domain.ValueObjects.Money(updateDto.MonthlyAmount));
                _budgetRepository.Update(budget);
                await _budgetRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el presupuesto", detail = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un presupuesto
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Budget? budget = await _budgetRepository.GetByIdAsync(id);
            if (budget == null)
                return NotFound(new { message = "Presupuesto no encontrado" });

            try
            {
                _budgetRepository.Delete(budget);
                await _budgetRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el presupuesto", detail = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene presupuestos por categoría
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<BudgetDTO>>> GetByCategory(int categoryId)
        {
            List<Budget> budgets = await _budgetRepository.GetAllAsync();
            IEnumerable<Budget> categoryBudgets = budgets.Where(b => b.CategoryId == categoryId);

            List<BudgetDTO> dtos = categoryBudgets.Select(b => new BudgetDTO
            {
                Id = b.Id,
                MonthlyAmount = b.MonthlyAmount.Value,
                Month = b.Period.Month,
                Year = b.Period.Year,
                CategoryId = b.CategoryId,
                //CategoryName = b.Category?.Name,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList();

            return Ok(dtos);
        }
    }
}
