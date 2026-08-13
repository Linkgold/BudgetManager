using Contracts.Enums;
using Shared.DTOs.Response.Dashboard;
using Shared.DTOs.Response.Data;
using Shared.DTOs.Response.HasData;
using System.Net;
using Tests.API.Fixtures;
using Tests.Helpers;

namespace Tests.API.Controllers
{
    /// <summary>
    /// Pruebas de integración para DataController
    /// </summary>
    [Collection("ApiTestCollection")]
    public class DataControllerTests : IClassFixture<ApiTestFixture>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ApiTestFixture _fixture;

        // Constantes para pruebas de HasData
        private const int TRANSACTION_YEAR_2023 = 2023;
        private const int TRANSACTION_YEAR_2024 = 2024;
        private const int BUDGET_YEAR_2023 = 2023;
        private const int BUDGET_YEAR_2024 = 2024;
        private const int FIXED_EXPENSE_YEAR_2024 = 2024;
        private static readonly List<int> EXPECTED_MONTHS_2023 = new() { 1, 2 };
        private static readonly List<int> EXPECTED_MONTHS_2024 = new() { 1 };
        private static readonly List<int> EXPECTED_MONTHS_2024_ONLY = new() { 1, 2, 3 };

        // Constantes para pruebas de Dashboard
        private const int DASHBOARD_YEAR = 2024;
        private const decimal EXPENSE_CATEGORY_BUDGET = 500.00m;
        private const decimal EXPENSE_CATEGORY_SPENT = 450.00m;
        private const decimal INCOME_CATEGORY_BUDGET = 2000.00m;
        private const decimal INCOME_CATEGORY_SPENT = 2000.00m;
        private const decimal EXPECTED_TOTAL_BUDGET_JANUARY = INCOME_CATEGORY_BUDGET - EXPENSE_CATEGORY_BUDGET; // 1500
        private const decimal EXPECTED_TOTAL_SPENT_JANUARY = INCOME_CATEGORY_SPENT - EXPENSE_CATEGORY_SPENT; // 1550
        private const int EXPECTED_CATEGORIES_COUNT = 2;
        private const int EXPECTED_TOTAL_MONTHS = 12;
        private const int INVALID_YEAR = 1899;

        public DataControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        public void Dispose()
        {
            _fixture.ClearDatabase();
        }

        // ================================================================
        // TESTS: GET HAS DATA
        // ================================================================

        [Fact]
        public async Task GetHasData_ReturnsOkWithHasDataResponseDTO()
        {
            // Arrange
            // Crear categoría
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            // Crear transacciones en diferentes años y meses
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra1", 100.00m, TransactionTypeEnum.Expense, "EUR", 15, 1, TRANSACTION_YEAR_2023);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra2", 200.00m, TransactionTypeEnum.Expense, "EUR", 20, 2, TRANSACTION_YEAR_2023);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra3", 150.00m, TransactionTypeEnum.Expense, "EUR", 10, 1, TRANSACTION_YEAR_2024);

            // Crear presupuestos
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 500.00m, "EUR", 1, BUDGET_YEAR_2023);
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 600.00m, "EUR", 1, BUDGET_YEAR_2024);

            // Crear gastos fijos
            await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "Netflix", 15.99m, "EUR", 1, FIXED_EXPENSE_YEAR_2024);

            int expectedTransactionYearsCount = 2;
            int expectedBudgetYearsCount = 2;
            int expectedFixedExpenseYearsCount = 1;

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/data/has-data");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            HasDataResponseDTO? result = _fixture.DeserializeResponse<HasDataResponseDTO>(content);

            Assert.NotNull(result);

            // Verificar TransactionMonthsByYear
            Assert.Equal(expectedTransactionYearsCount, result.TransactionMonthsByYear.Count);
            Assert.True(result.TransactionMonthsByYear.ContainsKey(TRANSACTION_YEAR_2023));
            Assert.True(result.TransactionMonthsByYear.ContainsKey(TRANSACTION_YEAR_2024));
            Assert.Equal(EXPECTED_MONTHS_2023, result.TransactionMonthsByYear[TRANSACTION_YEAR_2023]);
            Assert.Equal(EXPECTED_MONTHS_2024, result.TransactionMonthsByYear[TRANSACTION_YEAR_2024]);

            // Verificar BudgetYears
            Assert.Equal(expectedBudgetYearsCount, result.BudgetYears.Count);
            Assert.Contains(BUDGET_YEAR_2023, result.BudgetYears);
            Assert.Contains(BUDGET_YEAR_2024, result.BudgetYears);

            // Verificar FixedExpenseYears
            Assert.Equal(expectedFixedExpenseYearsCount, result.FixedExpenseYears.Count);
            Assert.Contains(FIXED_EXPENSE_YEAR_2024, result.FixedExpenseYears);
        }

        [Fact]
        public async Task GetHasData_WithNoData_ReturnsOkWithEmptyDTO()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/data/has-data");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            HasDataResponseDTO? result = _fixture.DeserializeResponse<HasDataResponseDTO>(content);

            Assert.NotNull(result);
            Assert.Empty(result.TransactionMonthsByYear);
            Assert.Empty(result.BudgetYears);
            Assert.Empty(result.FixedExpenseYears);
        }

        [Fact]
        public async Task GetHasData_WithTransactionsOnly_ReturnsOnlyTransactionsData()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            // Crear solo transacciones (sin presupuestos ni gastos fijos)
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra1", 100.00m, TransactionTypeEnum.Expense, "EUR", 15, 1, TRANSACTION_YEAR_2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra2", 200.00m, TransactionTypeEnum.Expense, "EUR", 20, 2, TRANSACTION_YEAR_2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra3", 150.00m, TransactionTypeEnum.Expense, "EUR", 10, 3, TRANSACTION_YEAR_2024);

            int expectedTransactionYearsCount = 1;

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/data/has-data");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            HasDataResponseDTO? result = _fixture.DeserializeResponse<HasDataResponseDTO>(content);

            Assert.NotNull(result);

            // Verificar TransactionMonthsByYear
            Assert.Equal(expectedTransactionYearsCount, result.TransactionMonthsByYear.Count);
            Assert.True(result.TransactionMonthsByYear.ContainsKey(TRANSACTION_YEAR_2024));
            Assert.Equal(EXPECTED_MONTHS_2024_ONLY, result.TransactionMonthsByYear[TRANSACTION_YEAR_2024]);

            // Verificar que BudgetYears y FixedExpenseYears están vacíos
            Assert.Empty(result.BudgetYears);
            Assert.Empty(result.FixedExpenseYears);
        }

        // ================================================================
        // TESTS: GET DASHBOARD DATA
        // ================================================================

        [Fact]
        public async Task GetDashboardData_WithValidYear_ReturnsOkWithDashboardResponseDTO()
        {
            // Arrange
            int year = DASHBOARD_YEAR;

            // Crear categorías
            int expenseCategoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int incomeCategoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Salario", CategoryNatureEnum.Income);

            // Crear presupuestos
            await TestDataFactory.CreateBudgetAsync(_fixture, expenseCategoryId, EXPENSE_CATEGORY_BUDGET, "EUR", 1, year);
            await TestDataFactory.CreateBudgetAsync(_fixture, incomeCategoryId, INCOME_CATEGORY_BUDGET, "EUR", 1, year);

            // Crear transacciones
            await TestDataFactory.CreateTransactionAsync(_fixture, expenseCategoryId, "Compra", EXPENSE_CATEGORY_SPENT, TransactionTypeEnum.Expense, "EUR", 15, 1, year);
            await TestDataFactory.CreateTransactionAsync(_fixture, incomeCategoryId, "Nómina", INCOME_CATEGORY_SPENT, TransactionTypeEnum.Income, "EUR", 1, 1, year);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/data/dashboard/{year}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            DashboardResponseDTO? result = _fixture.DeserializeResponse<DashboardResponseDTO>(content);

            Assert.NotNull(result);
            Assert.Equal(EXPECTED_CATEGORIES_COUNT, result.Categories.Count);
            Assert.Equal(EXPECTED_TOTAL_MONTHS, result.Months.Count);

            // Verificar categoría de gasto
            DashboardCategoryDTO expenseCategoryDto = result.Categories.First(c => c.CategoryName == "Alimentación");
            Assert.Equal(CategoryNatureEnum.Expense, expenseCategoryDto.Nature);
            Assert.Equal(-EXPENSE_CATEGORY_BUDGET, expenseCategoryDto.TotalBudget);
            Assert.Equal(-EXPENSE_CATEGORY_SPENT, expenseCategoryDto.TotalSpent);

            // Verificar categoría de ingreso
            DashboardCategoryDTO incomeCategoryDto = result.Categories.First(c => c.CategoryName == "Salario");
            Assert.Equal(CategoryNatureEnum.Income, incomeCategoryDto.Nature);
            Assert.Equal(INCOME_CATEGORY_BUDGET, incomeCategoryDto.TotalBudget);
            Assert.Equal(INCOME_CATEGORY_SPENT, incomeCategoryDto.TotalSpent);

            // Verificar totales de enero
            DashboardMonthTotalsDTO january = result.Months.First(m => m.Month == 1);
            Assert.Equal(EXPECTED_TOTAL_BUDGET_JANUARY, january.TotalBudget);
            Assert.Equal(EXPECTED_TOTAL_SPENT_JANUARY, january.TotalSpent);

            // Verificar acumulado de enero
            Assert.Equal(EXPECTED_TOTAL_BUDGET_JANUARY, january.AccumulatedBudget);
            Assert.Equal(EXPECTED_TOTAL_SPENT_JANUARY, january.AccumulatedSpent);
        }

        [Fact]
        public async Task GetDashboardData_WithNoData_ReturnsOkWithEmptyDashboard()
        {
            // Arrange
            int year = DASHBOARD_YEAR;

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/data/dashboard/{year}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            DashboardResponseDTO? result = _fixture.DeserializeResponse<DashboardResponseDTO>(content);

            Assert.NotNull(result);
            Assert.Empty(result.Categories);
            Assert.Equal(EXPECTED_TOTAL_MONTHS, result.Months.Count);
            Assert.All(result.Months, m => Assert.Equal(0m, m.TotalBudget));
            Assert.All(result.Months, m => Assert.Equal(0m, m.TotalSpent));
            Assert.All(result.Months, m => Assert.Equal(0m, m.AccumulatedBudget));
            Assert.All(result.Months, m => Assert.Equal(0m, m.AccumulatedSpent));
        }

        [Fact]
        public async Task GetDashboardData_WithInvalidYear_ReturnsBadRequest()
        {
            // Arrange
            int invalidYear = INVALID_YEAR;
            HttpStatusCode expectedStatusCode = HttpStatusCode.BadRequest;

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/data/dashboard/{invalidYear}");

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}