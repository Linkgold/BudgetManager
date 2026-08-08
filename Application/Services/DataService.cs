using Application.Interfaces;
using AutoMapper;
using Contracts.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Shared.DTOs.Response.Dashboard;
using Shared.DTOs.Response.Data;
using Shared.DTOs.Response.HasData;
using Shared.DTOs.Response.MonthDetail;

namespace Application.Services.Data
{
    public class DataService : IDataService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IFixedExpenseRepository _fixedExpenseRepository;
        private readonly IMapper _mapper;

        public DataService
        (
            ICurrentUserService currentUserService,
            ICategoryRepository categoryRepository,
            IBudgetRepository budgetRepository,
            ITransactionRepository transactionRepository,
            IFixedExpenseRepository fixedExpenseRepository,
            IMapper mapper
        )
        {
            _currentUserService = currentUserService;
            _categoryRepository = categoryRepository;
            _budgetRepository = budgetRepository;
            _transactionRepository = transactionRepository;
            _fixedExpenseRepository = fixedExpenseRepository;
            _mapper = mapper;
        }

        private int UserId => _currentUserService.UserId;

        // ================================================================
        // DASHBOARD
        // ================================================================

        public async Task<DashboardResponseDTO> GetDashboardDataAsync(int year)
        {
            DashboardResponseDTO dashboard = new DashboardResponseDTO();

            // 1. Obtener datos
            IEnumerable<Category> categories = await _categoryRepository.GetAllAsync(UserId);
            IEnumerable<Budget> budgets = await _budgetRepository.GetAllByYearAsync(UserId, year);
            IEnumerable<Transaction> transactions = await _transactionRepository.GetAllByYearAsync(UserId, year);
            IEnumerable<FixedExpense> fixedExpenses = await _fixedExpenseRepository.GetAllByYearAsync(UserId, year);

            if(categories == null || budgets == null || transactions == null || fixedExpenses == null)
            {
                throw new Exception("Error obtaining dashboard data.");
            }

            // 2. Inicializar meses
            List<DashboardMonthTotalsDTO> months = new List<DashboardMonthTotalsDTO>();
            for (int month = 1; month <= 12; month++)
            {
                months.Add(new DashboardMonthTotalsDTO
                {
                    Month = month,
                    TotalBudget = 0m,
                    TotalSpent = 0m,
                    AccumulatedBudget = 0m,
                    AccumulatedSpent = 0m
                });
            }

            // 3. Procesar cada categoría
            foreach (Category category in categories)
            {
                DashboardCategoryDTO categoryDto = new DashboardCategoryDTO
                {
                    CategoryName = category.Info.Name,
                    Nature = category.Nature,
                    MonthlyData = new List<DashboardCategoryMonthDTO>(),
                    TotalBudget = 0m,
                    TotalSpent = 0m
                };

                for (int month = 1; month <= 12; month++)
                {
                    // Presupuesto de la categoría en este mes
                    decimal budget = budgets
                        .Where(b => b.CategoryId == category.Id && b.Period.Month == month && b.Period.Year == year)
                        .Sum(b => b.Category.Nature == CategoryNatureEnum.Income ? b.MonthlyAmount.Value : -b.MonthlyAmount.Value);

                    // Gasto fijo de la categoría en este mes
                    decimal fixedExpense = fixedExpenses
                        .Where(f => f.CategoryId == category.Id && f.ChargePeriod.Month == month && f.ChargePeriod.Year == year)
                        .Sum(f => -f.Amount.Value);

                    // Transacciones de la categoría en este mes
                    decimal spent = transactions
                        .Where(t => t.CategoryId == category.Id && t.Date.Month == month && t.Date.Year == year)
                        .Sum(t => t.TransactionType == TransactionTypeEnum.Income ? t.Amount.Value : -t.Amount.Value);

                    // Total presupuesto del mes (presupuesto + gasto fijo)
                    decimal totalBudget = budget + fixedExpense;
                    decimal totalSpent = spent;

                    // Guardar datos del mes
                    categoryDto.MonthlyData.Add(new DashboardCategoryMonthDTO
                    {
                        Month = month,
                        Budget = totalBudget,
                        Spent = totalSpent
                    });

                    // Acumular totales de la categoría
                    categoryDto.TotalBudget += totalBudget;
                    categoryDto.TotalSpent += totalSpent;

                    // Acumular totales del mes (todas las categorías)
                    DashboardMonthTotalsDTO monthDto = months.First(m => m.Month == month);
                    monthDto.TotalBudget += totalBudget;
                    monthDto.TotalSpent += totalSpent;
                }

                dashboard.Categories.Add(categoryDto);
            }

            decimal accumulatedBudget = 0m;
            decimal accumulatedSpent = 0m;

            // 4. Calcular totales por mes y acumulados (aplicando signo según naturaleza)
            foreach (DashboardMonthTotalsDTO month in months)
            {
                decimal monthBudgetWithSign = 0m;
                decimal monthSpentWithSign = 0m;

                foreach (DashboardCategoryDTO category in dashboard.Categories)
                {
                    DashboardCategoryMonthDTO? monthData = category.MonthlyData.FirstOrDefault(m => m.Month == month.Month);

                    if (monthData != null)
                    {
                        monthBudgetWithSign += monthData.Budget;
                        monthSpentWithSign += monthData.Spent;
                    }
                }

                month.TotalBudget = monthBudgetWithSign;
                month.TotalSpent = monthSpentWithSign;

                accumulatedBudget += monthBudgetWithSign;
                accumulatedSpent += monthSpentWithSign;

                month.AccumulatedBudget = accumulatedBudget;
                month.AccumulatedSpent = accumulatedSpent;
            }

            dashboard.Months = months;

            return dashboard;
        }

        // ================================================================
        // MONTH DETAIL
        // ================================================================

        public async Task<MonthDetailResponseDTO> GetMonthDetailAsync(int year, int month)
        {
            MonthDetailResponseDTO monthDetail = new MonthDetailResponseDTO();

            // TODO: Obtener datos del mes

            return monthDetail;
        }

        // ================================================================
        // HAS DATA
        // ================================================================

        public async Task<HasDataResponseDTO> GetHasDataAsync()
        {
            HasDataResponseDTO hasData = new HasDataResponseDTO();

            // TODO: Obtener años con datos

            return hasData;
        }
    }
}