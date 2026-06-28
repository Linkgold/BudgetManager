using Application.DTOs;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly ExpenseRepository _expenseRepository;
        private readonly BudgetRepository _budgetRepository;

        public ExpensesController(ExpenseRepository expenseRepository, BudgetRepository budgetRepository)
        {
            _expenseRepository = expenseRepository;
            _budgetRepository = budgetRepository;
        }

        /// <summary>
        /// Obtiene todos los gastos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> GetAll()
        {
            List<Expense> expenses = await _expenseRepository.GetAllAsync();
            List<ExpenseDTO> dtos = expenses.Select(e => new ExpenseDTO
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount.Value,
                DateTime = e.DateTime,
                Category = e.Category,
                Notes = e.Notes,
                BudgetId = e.BudgetId,
                CreatedAt = e.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene un gasto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseDTO>> GetById(int id)
        {
            Expense? expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
                return NotFound(new { message = "Gasto no encontrado" });

            ExpenseDTO dto = new ExpenseDTO
            {
                Id = expense.Id,
                Description = expense.Description,
                Amount = expense.Amount.Value,
                DateTime = expense.DateTime,
                Category = expense.Category,
                Notes = expense.Notes,
                BudgetId = expense.BudgetId,
                CreatedAt = expense.CreatedAt
            };

            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo gasto
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ExpenseDTO>> Create([FromBody] CreateExpenseDTO createDto)
        {
            /*if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Expense expense = new Expense(
                    createDto.Description,
                    new Money(createDto.Amount),
                    createDto.DateTime,
                    createDto.Category,
                    createDto.Notes
                );

                // If BudgetId is provided, assign the budget
                if (createDto.BudgetId.HasValue)
                {
                    Budget? budget = await _budgetRepository.GetByIdAsync(createDto.BudgetId.Value);
                    if (budget == null)
                        return BadRequest(new { message = "Presupuesto no encontrado" });

                    expense.AssignBudget(budget);
                }

                await _expenseRepository.AddAsync(expense);
                await _expenseRepository.SaveChangesAsync();

                ExpenseDTO dto = new ExpenseDTO
                {
                    Id = expense.Id,
                    Description = expense.Description,
                    Amount = expense.Amount.Value,
                    DateTime = expense.DateTime,
                    Category = expense.Category,
                    Notes = expense.Notes,
                    BudgetId = expense.BudgetId,
                    CreatedAt = expense.CreatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = expense.Id }, dto);
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
                return StatusCode(500, new { message = "Error al crear el gasto", detail = ex.Message });
            }*/

            return null;
        }

        /// <summary>
        /// Obtiene gastos por presupuesto
        /// </summary>
        [HttpGet("budget/{budgetId}")]
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> GetByBudget(int budgetId)
        {
            List<Expense> expenses = await _expenseRepository.GetAllAsync();
            IEnumerable<Expense> budgetExpenses = expenses.Where(e => e.BudgetId == budgetId);

            List<ExpenseDTO> dtos = budgetExpenses.Select(e => new ExpenseDTO
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount.Value,
                DateTime = e.DateTime,
                Category = e.Category,
                Notes = e.Notes,
                BudgetId = e.BudgetId,
                CreatedAt = e.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene gastos por mes y año
        /// </summary>
        [HttpGet("by-period/{month}/{year}")]
        public async Task<ActionResult<IEnumerable<ExpenseDTO>>> GetByPeriod(int month, int year)
        {
            List<Expense> expenses = await _expenseRepository.GetAllAsync();
            IEnumerable<Expense> periodExpenses = expenses.Where(e => e.DateTime.Month == month && e.DateTime.Year == year);

            List<ExpenseDTO> dtos = periodExpenses.Select(e => new ExpenseDTO
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount.Value,
                DateTime = e.DateTime,
                Category = e.Category,
                Notes = e.Notes,
                BudgetId = e.BudgetId,
                CreatedAt = e.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Elimina un gasto
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Expense? expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null)
                return NotFound(new { message = "Gasto no encontrado" });

            try
            {
                _expenseRepository.Delete(expense);
                await _expenseRepository.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el gasto", detail = ex.Message });
            }
        }
    }
}
