using Application.Interfaces;
using Application.Mappings;
using Application.Services.Data;
using AutoMapper;
using Contracts.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.DTOs.Response.Dashboard;
using Shared.DTOs.Response.Data;
using Shared.DTOs.Response.HasData;
using Tests.Helpers;

namespace Tests.Application
{
    public class DataServiceTests : IDisposable
    {
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IFixedExpenseRepository> _fixedExpenseRepositoryMock;
        private readonly IMapper _mapper;
        private readonly IDataService _dataService;

        // Constantes para pruebas de Dashboard
        private const int TEST_USER_ID = 1;
        private const int TEST_YEAR = 2024;
        private const decimal EXPENSE_CATEGORY_BUDGET = 500.00m;
        private const decimal EXPENSE_CATEGORY_SPENT = 450.00m;
        private const decimal INCOME_CATEGORY_BUDGET = 2000.00m;
        private const decimal INCOME_CATEGORY_SPENT = 2000.00m;
        private const decimal EXPECTED_TOTAL_BUDGET_JANUARY = INCOME_CATEGORY_BUDGET - EXPENSE_CATEGORY_BUDGET; // 1500
        private const decimal EXPECTED_TOTAL_SPENT_JANUARY = INCOME_CATEGORY_SPENT - EXPENSE_CATEGORY_SPENT; // 1550

        // Constantes para pruebas de HasData
        private const int TRANSACTION_YEAR_2023 = 2023;
        private const int TRANSACTION_YEAR_2024 = 2024;
        private const int BUDGET_YEAR_2023 = 2023;
        private const int BUDGET_YEAR_2024 = 2024;
        private const int FIXED_EXPENSE_YEAR_2024 = 2024;
        private static readonly List<int> EXPECTED_MONTHS_2023 = new() { 1, 2 };
        private static readonly List<int> EXPECTED_MONTHS_2024 = new() { 1 };

        public DataServiceTests()
        {
            // Configurar AutoMapper
            MapperConfiguration mapperConfiguration = new MapperConfiguration
            (
                config =>
                {
                    config.AddProfile<AutoMapperProfile>();
                },
                new LoggerFactory()
            );

            _mapper = mapperConfiguration.CreateMapper();

            // Crear mocks
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _fixedExpenseRepositoryMock = new Mock<IFixedExpenseRepository>();

            // Instanciar el servicio
            _dataService = new DataService
            (
                _currentUserServiceMock.Object,
                _categoryRepositoryMock.Object,
                _budgetRepositoryMock.Object,
                _transactionRepositoryMock.Object,
                _fixedExpenseRepositoryMock.Object,
                _mapper
            );
        }

        // ================================================================
        // TESTS: GET DASHBOARD DATA
        // ================================================================

        [Fact]
        public async Task GetDashboardDataAsync_WithData_ReturnsDashboardResponseDTO()
        {
            // Arrange
            int userId = TEST_USER_ID;
            int year = TEST_YEAR;
            int expectedCategoriesCount = 2;
            int expectedTotalMonths = 12;
            decimal expectedAmount0 = 0m;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user, "Alimentación", nature: CategoryNatureEnum.Expense);
            Category incomeCategory = TestDataFactory.CreateCategory(2, user, "Salario", nature: CategoryNatureEnum.Income);

            List<Category> categories = new List<Category> { category, incomeCategory };

            List<Budget> budgets = new List<Budget>
            {
                TestDataFactory.CreateBudget(1, user, category, EXPENSE_CATEGORY_BUDGET, "EUR", 1, year),
                TestDataFactory.CreateBudget(2, user, incomeCategory, INCOME_CATEGORY_BUDGET, "EUR", 1, year)
            };

            List<Transaction> transactions = new List<Transaction>
            {
                TestDataFactory.CreateTransaction(1, user, category, "Compra", "Supermercado", EXPENSE_CATEGORY_SPENT, TransactionTypeEnum.Expense, "EUR", 15, 1, year),
                TestDataFactory.CreateTransaction(2, user, incomeCategory, "Nómina", "Salario enero", INCOME_CATEGORY_SPENT, TransactionTypeEnum.Income, "EUR", 1, 1, year)
            };

            List<FixedExpense> fixedExpenses = new List<FixedExpense>();

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync(categories);

            _budgetRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(budgets);

            _transactionRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(transactions);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(fixedExpenses);

            // Act
            DashboardResponseDTO result = await _dataService.GetDashboardDataAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCategoriesCount, result.Categories.Count);
            Assert.Equal(expectedTotalMonths, result.Months.Count);

            // Verificar categoría de gasto (Alimentación)
            DashboardCategoryDTO expenseCategory = result.Categories.First(c => c.CategoryName == "Alimentación");
            Assert.Equal(CategoryNatureEnum.Expense, expenseCategory.Nature);
            Assert.Equal(-EXPENSE_CATEGORY_BUDGET, expenseCategory.TotalBudget);
            Assert.Equal(-EXPENSE_CATEGORY_SPENT, expenseCategory.TotalSpent);

            // Verificar categoría de ingreso (Salario)
            DashboardCategoryDTO incomeCategoryDto = result.Categories.First(c => c.CategoryName == "Salario");
            Assert.Equal(CategoryNatureEnum.Income, incomeCategoryDto.Nature);
            Assert.Equal(INCOME_CATEGORY_BUDGET, incomeCategoryDto.TotalBudget);
            Assert.Equal(INCOME_CATEGORY_SPENT, incomeCategoryDto.TotalSpent);

            // Verificar totales del mes de enero
            DashboardMonthTotalsDTO january = result.Months.First(m => m.Month == 1);
            Assert.Equal(EXPECTED_TOTAL_BUDGET_JANUARY, january.TotalBudget);
            Assert.Equal(EXPECTED_TOTAL_SPENT_JANUARY, january.TotalSpent);

            // Verificar acumulado de enero
            Assert.Equal(EXPECTED_TOTAL_BUDGET_JANUARY, january.AccumulatedBudget);
            Assert.Equal(EXPECTED_TOTAL_SPENT_JANUARY, january.AccumulatedSpent);

            // Verificar meses sin datos (febrero)
            DashboardMonthTotalsDTO february = result.Months.First(m => m.Month == 2);
            Assert.Equal(expectedAmount0, february.TotalBudget);
            Assert.Equal(expectedAmount0, february.TotalSpent);
            Assert.Equal(EXPECTED_TOTAL_BUDGET_JANUARY, february.AccumulatedBudget);
            Assert.Equal(EXPECTED_TOTAL_SPENT_JANUARY, february.AccumulatedSpent);
        }

        [Fact]
        public async Task GetDashboardDataAsync_WithNoCategories_ReturnsEmptyDashboard()
        {
            // Arrange
            int userId = TEST_USER_ID;
            int year = TEST_YEAR;
            int expectedTotalMonths = 12;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync(new List<Category>());

            _budgetRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(new List<Budget>());

            _transactionRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(new List<Transaction>());

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(new List<FixedExpense>());

            // Act
            DashboardResponseDTO result = await _dataService.GetDashboardDataAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Categories);
            Assert.Equal(expectedTotalMonths, result.Months.Count);
            Assert.All(result.Months, m => Assert.Equal(0m, m.TotalBudget));
            Assert.All(result.Months, m => Assert.Equal(0m, m.TotalSpent));
            Assert.All(result.Months, m => Assert.Equal(0m, m.AccumulatedBudget));
            Assert.All(result.Months, m => Assert.Equal(0m, m.AccumulatedSpent));
        }

        [Fact]
        public async Task GetDashboardDataAsync_WithMixedCategories_CalculatesCorrectTotals()
        {
            // Arrange
            int userId = TEST_USER_ID;
            int year = TEST_YEAR;
            User user = TestDataFactory.CreateUser(userId);

            decimal expenseBudget = 1000.00m;
            decimal expenseSpent = 800.00m;
            decimal incomeBudget = 1500.00m;
            decimal incomeSpent = 1500.00m;
            decimal expectedTotalBudget = incomeBudget - expenseBudget; // 500
            decimal expectedTotalSpent = incomeSpent - expenseSpent; // 700
            string expectedIncomneCategoryName = "Salario";

            Category expenseCategory = TestDataFactory.CreateCategory(1, user, nature:CategoryNatureEnum.Expense);
            Category incomeCategory = TestDataFactory.CreateCategory(2, user, expectedIncomneCategoryName, nature: CategoryNatureEnum.Income);

            List<Category> categories = new List<Category> { expenseCategory, incomeCategory };

            List<Budget> budgets = new List<Budget>
            {
                TestDataFactory.CreateBudget(1, user, expenseCategory, expenseBudget, "EUR", 1, year),
                TestDataFactory.CreateBudget(2, user, incomeCategory, incomeBudget, "EUR", 1, year)
            };

            List<Transaction> transactions = new List<Transaction>
            {
                TestDataFactory.CreateTransaction(1, user, expenseCategory, "Compra", "Supermercado", expenseSpent, TransactionTypeEnum.Expense, "EUR", 15, 1, year),
                TestDataFactory.CreateTransaction(2, user, incomeCategory, "Nómina", expectedIncomneCategoryName, incomeSpent, TransactionTypeEnum.Income, "EUR", 1, 1, year)
            };

            List<FixedExpense> fixedExpenses = new List<FixedExpense>();

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync(categories);

            _budgetRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(budgets);

            _transactionRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(transactions);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetAllByYearAsync(userId, year))
                .ReturnsAsync(fixedExpenses);

            // Act
            DashboardResponseDTO result = await _dataService.GetDashboardDataAsync(year);

            // Assert
            Assert.NotNull(result);

            // Verificar totales de enero: Ingresos - Gastos
            DashboardMonthTotalsDTO january = result.Months.First(m => m.Month == 1);
            Assert.Equal(expectedTotalBudget, january.TotalBudget);
            Assert.Equal(expectedTotalSpent, january.TotalSpent);
            Assert.Equal(expectedTotalBudget, january.AccumulatedBudget);
            Assert.Equal(expectedTotalSpent, january.AccumulatedSpent);

            // Verificar totales de categoría individual
            DashboardCategoryDTO expenseCategoryDto = result.Categories.First(c => c.CategoryName == TestDataFactory.DEFAULT_CATEGORY_NAME);
            Assert.Equal(-expenseBudget, expenseCategoryDto.TotalBudget);
            Assert.Equal(-expenseSpent, expenseCategoryDto.TotalSpent);

            DashboardCategoryDTO incomeCategoryDto = result.Categories.First(c => c.CategoryName == expectedIncomneCategoryName);
            Assert.Equal(incomeBudget, incomeCategoryDto.TotalBudget);
            Assert.Equal(incomeSpent, incomeCategoryDto.TotalSpent);
        }

        // ================================================================
        // TESTS: GET HAS DATA
        // ================================================================

        [Fact]
        public async Task GetHasDataAsync_WithData_ReturnsHasDataResponseDTO()
        {
            // Arrange
            int userId = TEST_USER_ID;
            int expectedTransactionYearsCount = 2;
            int expectedBudgetsYearsCount = 2;
            User user = TestDataFactory.CreateUser(userId);

            List<Transaction> transactions = new List<Transaction>
            {
                TestDataFactory.CreateTransaction(1, user, TestDataFactory.CreateCategory(), "Compra1", "", 100.00m, TransactionTypeEnum.Expense, "EUR", 15, 1, TRANSACTION_YEAR_2023),
                TestDataFactory.CreateTransaction(2, user, TestDataFactory.CreateCategory(), "Compra2", "", 100.00m, TransactionTypeEnum.Expense, "EUR", 15, 2, TRANSACTION_YEAR_2023),
                TestDataFactory.CreateTransaction(3, user, TestDataFactory.CreateCategory(), "Compra3", "", 100.00m, TransactionTypeEnum.Expense, "EUR", 15, 1, TRANSACTION_YEAR_2024)
            };

            List<Budget> budgets = new List<Budget>
            {
                TestDataFactory.CreateBudget(1, user, TestDataFactory.CreateCategory(), 100.00m, "EUR", 1, BUDGET_YEAR_2023),
                TestDataFactory.CreateBudget(2, user, TestDataFactory.CreateCategory(), 100.00m, "EUR", 1, BUDGET_YEAR_2024)
            };

            List<FixedExpense> fixedExpenses = new List<FixedExpense>
            {
                TestDataFactory.CreateFixedExpense(1, user, TestDataFactory.CreateCategory(), "Gasto1", "", 50.00m, "EUR", 1, FIXED_EXPENSE_YEAR_2024)
            };

            Dictionary<int, List<int>> transactionMonthsByYear = new Dictionary<int, List<int>>
            {
                { TRANSACTION_YEAR_2023, EXPECTED_MONTHS_2023 },
                { TRANSACTION_YEAR_2024, EXPECTED_MONTHS_2024 }
            };

            List<int> expectedBudgetYears = new List<int> { BUDGET_YEAR_2023, BUDGET_YEAR_2024 };
            List<int> expectedFixedExpenseYears = new List<int> { FIXED_EXPENSE_YEAR_2024 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAndMonthsAsync(userId))
                .ReturnsAsync(transactionMonthsByYear);

            _budgetRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAsync(userId))
                .ReturnsAsync(expectedBudgetYears);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAsync(userId))
                .ReturnsAsync(expectedFixedExpenseYears);

            // Act
            HasDataResponseDTO result = await _dataService.GetHasDataAsync();

            // Assert
            Assert.NotNull(result);

            // Verificar TransactionMonthsByYear
            Assert.Equal(expectedTransactionYearsCount, result.TransactionMonthsByYear.Count);
            Assert.True(result.TransactionMonthsByYear.ContainsKey(TRANSACTION_YEAR_2023));
            Assert.True(result.TransactionMonthsByYear.ContainsKey(TRANSACTION_YEAR_2024));
            Assert.Equal(EXPECTED_MONTHS_2023, result.TransactionMonthsByYear[TRANSACTION_YEAR_2023]);
            Assert.Equal(EXPECTED_MONTHS_2024, result.TransactionMonthsByYear[TRANSACTION_YEAR_2024]);

            // Verificar BudgetYears
            Assert.Equal(expectedBudgetsYearsCount, result.BudgetYears.Count);
            Assert.Contains(BUDGET_YEAR_2023, result.BudgetYears);
            Assert.Contains(BUDGET_YEAR_2024, result.BudgetYears);

            // Verificar FixedExpenseYears
            Assert.Single(result.FixedExpenseYears);
            Assert.Contains(FIXED_EXPENSE_YEAR_2024, result.FixedExpenseYears);
        }

        [Fact]
        public async Task GetHasDataAsync_WithNoData_ReturnsEmptyHasDataResponseDTO()
        {
            // Arrange
            int userId = TEST_USER_ID;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAndMonthsAsync(userId))
                .ReturnsAsync(new Dictionary<int, List<int>>());

            _budgetRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAsync(userId))
                .ReturnsAsync(new List<int>());

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAsync(userId))
                .ReturnsAsync(new List<int>());

            // Act
            HasDataResponseDTO result = await _dataService.GetHasDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.TransactionMonthsByYear);
            Assert.Empty(result.BudgetYears);
            Assert.Empty(result.FixedExpenseYears);
        }

        [Fact]
        public async Task GetHasDataAsync_WithTransactionsOnly_ReturnsOnlyTransactionsData()
        {
            // Arrange
            int userId = TEST_USER_ID;
            List<int> expectedMonths = new List<int> { 1, 2, 3 };

            Dictionary<int, List<int>> transactionMonthsByYear = new Dictionary<int, List<int>>
            {
                { TRANSACTION_YEAR_2024, expectedMonths }
            };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAndMonthsAsync(userId))
                .ReturnsAsync(transactionMonthsByYear);

            _budgetRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAsync(userId))
                .ReturnsAsync(new List<int>());

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetDistinctYearsAsync(userId))
                .ReturnsAsync(new List<int>());

            // Act
            HasDataResponseDTO result = await _dataService.GetHasDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.TransactionMonthsByYear);
            Assert.True(result.TransactionMonthsByYear.ContainsKey(TRANSACTION_YEAR_2024));
            Assert.Equal(expectedMonths, result.TransactionMonthsByYear[TRANSACTION_YEAR_2024]);
            Assert.Empty(result.BudgetYears);
            Assert.Empty(result.FixedExpenseYears);
        }

        // ================================================================
        // DISPOSE
        // ================================================================

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}