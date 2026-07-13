using Application.DTOs.Request;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using Moq;

namespace Tests.Helpers
{
    /// <summary>
    /// Fábrica de datos para pruebas unitarias de todas las capas
    /// </summary>
    public static class TestDataFactory
    {
        // ==================== CONSTANTES DE PRUEBA ====================

        public const string DEFAULT_USER_NAME = "Toribio Gaviria";
        public const string DEFAULT_USER_EMAIL = "torigavi@email.com";
        public const string DEFAULT_PASSWORD = "Password123!";
        public const string DEFAULT_CATEGORY_NAME = "Alimentación";
        public const string DEFAULT_CATEGORY_DESCRIPTION = "Gastos de comida";
        public const string DEFAULT_CATEGORIES_NAME = "Categoría ";
        public const string DEFAULT_CATEGORIES_DESCRIPTION = "Descripción ";
        public const string DEFAULT_FIXED_EXPENSE_NAME = "Netflix";
        public const string DEFAULT_FIXED_EXPENSE_DESCRIPTION = "Suscripción mensual";
        public const decimal DEFAULT_FIXED_EXPENSE_AMOUNT = 15.90m;
        public const decimal DEFAULT_BUDGET_AMOUNT = 500.00m;
        public const int DEFAULT_MONTHLY_MONTH = 1;
        public const int DEFAULT_YEAR = 2024;
        public const int DEFAULT_DAILY_DAY = 15;
        public const string DEFAULT_TRANSACTION_NAME = "Compra supermercado";
        public const string DEFAULT_TRANSACTION_DESCRIPTION = "Carrefour 15/06/2024";
        public const decimal DEFAULT_TRANSACTION_AMOUNT = 45.75m;
        public const int DEFAULT_DAILY_MONTH = 6;
        public const string DEFAULT_ENTITY_INFO_NAME = "Comida";
        public const string DEFAULT_ENTITY_INFO_DESCRIPTION = "Gastos de supermercado";
        public const string DEFAULT_CURRENCY = "EUR";

        // ==================== USUARIOS ====================

        /// <summary>
        /// Crea un usuario de prueba con ID
        /// </summary>
        public static User CreateUser(int id = 1, string userName = DEFAULT_USER_NAME, string email = DEFAULT_USER_EMAIL)
        {
            UserInfo userInfo = new UserInfo(userName, email);
            User user = new User(userInfo, "hashed_password");
            typeof(User).GetProperty("Id")?.SetValue(user, id);

            return user;
        }

        public static User CreateUser(UserInfo userInfo, string passwordHash) => new(userInfo, passwordHash);

        /// <summary>
        /// Crea un usuario con contraseña hasheada (para pruebas de login)
        /// </summary>
        public static User CreateUserWithPassword(string password = DEFAULT_PASSWORD, int id = 1, string email = DEFAULT_USER_EMAIL)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            UserInfo userInfo = new UserInfo(DEFAULT_USER_NAME, email);
            User user = new User(userInfo, passwordHash);
            typeof(User).GetProperty("Id")?.SetValue(user, id);

            return user;
        }

        // ==================== CATEGORÍAS ====================

        /// <summary>
        /// Crea una categoría de prueba con usuario
        /// </summary>
        public static Category CreateCategory(int id = 1, User user = null, string name = DEFAULT_CATEGORY_NAME, string description = DEFAULT_CATEGORY_DESCRIPTION)
        {
            user ??= CreateUser(1);
            EntityInfo info = new EntityInfo(name, description);
            Category category = new Category(user, info);
            typeof(Category).GetProperty("Id")?.SetValue(category, id);

            return category;
        }

        /// <summary>
        /// Crea una lista de categorías de prueba
        /// </summary>
        public static List<Category> CreateCategories(int count = 3, User user = null)
        {
            user ??= CreateUser(1);
            List<Category> categories = new List<Category>();
            for (int i = 1; i <= count; i++)
            {
                categories.Add(CreateCategory(i, user, $"{DEFAULT_CATEGORIES_NAME} {i}", $"{DEFAULT_CATEGORIES_DESCRIPTION} {i}"));
            }
            return categories;
        }

        // ==================== GASTOS FIJOS ====================

        /// <summary>
        /// Crea un gasto fijo de prueba
        /// </summary>
        public static FixedExpense CreateFixedExpense
        (
            int id = 1,
            User user = null,
            Category category = null,
            string name = DEFAULT_FIXED_EXPENSE_NAME,
            string description = DEFAULT_FIXED_EXPENSE_DESCRIPTION,
            decimal amount = DEFAULT_FIXED_EXPENSE_AMOUNT,
            int month = DEFAULT_MONTHLY_MONTH,
            int year = DEFAULT_YEAR
        )
        {
            user ??= CreateUser(1);
            category ??= CreateCategory(1, user);
            EntityInfo info = new EntityInfo(name, description);
            Money money = new Money(amount);
            MonthlyPeriod period = new MonthlyPeriod(month, year);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, money, period);
            typeof(FixedExpense).GetProperty("Id")?.SetValue(fixedExpense, id);
            return fixedExpense;
        }

        public static FixedExpense CreateFixedExpense(User user, Category category, EntityInfo info, Money money, MonthlyPeriod monthlyPeriod) => new(user, category, info, money, monthlyPeriod);

        // ==================== PRESUPUESTOS ====================

        /// <summary>
        /// Crea un presupuesto de prueba
        /// </summary>
        public static Budget CreateBudget
        (
            int id = 1,
            User user = null,
            Category category = null,
            decimal monthlyAmount = DEFAULT_BUDGET_AMOUNT,
            int month = DEFAULT_MONTHLY_MONTH,
            int year = DEFAULT_YEAR
        )
        {
            user ??= CreateUser(1);
            category ??= CreateCategory(1, user);
            Money money = new Money(monthlyAmount);
            MonthlyPeriod period = new MonthlyPeriod(month, year);
            Budget budget = new Budget(user, category, money, period);
            typeof(Budget).GetProperty("Id")?.SetValue(budget, id);
            return budget;
        }

        public static Budget CreateBudget(User user, Category category, Money money, MonthlyPeriod monthlyPeriod) => new(user, category, money, monthlyPeriod);

        // ==================== TRANSACCIONES ====================

        /// <summary>
        /// Crea una transacción de prueba
        /// </summary>
        public static Transaction CreateTransaction
        (
            int id = 1,
            User user = null,
            Category category = null,
            string name = DEFAULT_TRANSACTION_NAME,
            string description = DEFAULT_TRANSACTION_DESCRIPTION,
            decimal amount = 45.75m,
            TransactionTypeEnum type = TransactionTypeEnum.Expense,
            int day = DEFAULT_DAILY_DAY,
            int month = DEFAULT_DAILY_MONTH,
            int year = DEFAULT_YEAR
        )
        {
            user ??= CreateUser(1);
            category ??= CreateCategory(1, user);
            EntityInfo info = new EntityInfo(name, description);
            Money money = new Money(amount);
            DailyPeriod date = new DailyPeriod(day, month, year);
            Transaction transaction = new Transaction(user, category, info, money, type, date);
            typeof(Transaction).GetProperty("Id")?.SetValue(transaction, id);
            return transaction;
        }

        public static Transaction CreateTransaction(User user, Category category, EntityInfo info, Money money, TransactionTypeEnum type, DailyPeriod date) => new(user, category, info, money, type, date);

        // ==================== VALUE OBJECTS ====================

        /// <summary>
        /// Crea un EntityInfo de prueba
        /// </summary>
        public static EntityInfo CreateEntityInfo(string name = DEFAULT_ENTITY_INFO_NAME, string description = DEFAULT_ENTITY_INFO_DESCRIPTION) => new EntityInfo(name, description);

        /// <summary>
        /// Crea un UserInfo de prueba
        /// </summary>
        public static UserInfo CreateUserInfo(string userName = DEFAULT_USER_NAME, string email = DEFAULT_USER_EMAIL) => new UserInfo(userName, email);

        /// <summary>
        /// Crea un DailyPeriod de prueba
        /// </summary>
        public static DailyPeriod CreateDailyPeriod(int day = DEFAULT_DAILY_DAY, int month = DEFAULT_DAILY_MONTH, int year = DEFAULT_YEAR) => new DailyPeriod(day, month, year);

        /// <summary>
        /// Crea un MonthlyPeriod de prueba
        /// </summary>
        public static MonthlyPeriod CreateMonthlyPeriod(int month = DEFAULT_MONTHLY_MONTH, int year = DEFAULT_YEAR) => new MonthlyPeriod(month, year);

        /// <summary>
        /// Crea un Money de prueba
        /// </summary>
        public static Money CreateMoney(decimal value = 100.00m, string currency = DEFAULT_CURRENCY) => new Money(value, currency);

        // Seeds

        public static async Task<Category> SeedCategoryAsync(ICategoryRepository repository, Category? category = null)
        {
            if (category == null) { category = CreateCategory(); }

            await repository.AddAsync(category);

            return category;
        }
        public static async Task<FixedExpense> SeedFixedExpenseAsync(IFixedExpenseRepository repository, FixedExpense? fixedExpense=null)
        {
            if (fixedExpense == null) { fixedExpense = CreateFixedExpense(); }

            await repository.AddAsync(fixedExpense);

            return fixedExpense;
        }

        public static async Task<Budget> SeedBudgetAsync(IBudgetRepository repository, Category? category = null, Budget? budget = null)
        {
            if (category == null) { category = CreateCategory(); }
            if (budget == null) { budget = CreateBudget(); }

            await repository.AddAsync(budget);

            return budget;
        }

        public static async Task<Transaction> SeedTransactionAsync(ITransactionRepository repository, Category? category = null, Transaction? transaction = null)
        {
            if (category == null) { category = CreateCategory(); }
            if (transaction == null) { transaction = CreateTransaction(); }

            await repository.AddAsync(transaction);

            return transaction;
        }

        // ==================== MOCKS (para Application e Infrastructure) ====================

        /// <summary>
        /// Configura un mock de ICurrentUserService para simular un usuario autenticado
        /// </summary>
        public static void SetupAuthenticatedUser(Mock<ICurrentUserService> mock, int userId = 1)
        {
            mock.Setup(service => service.UserId).Returns(userId);
            mock.Setup(service => service.IsAuthenticated).Returns(true);
            mock.Setup(service => service.UserName).Returns("Juan Pérez");
            mock.Setup(service => service.Email).Returns("juan@email.com");
        }

        /// <summary>
        /// Configura un mock de ICurrentUserService para simular un usuario no autenticado
        /// </summary>
        public static void SetupUnauthenticatedUser(Mock<ICurrentUserService> mock)
        {
            mock.Setup(service => service.UserId).Returns(0);
            mock.Setup(service => service.IsAuthenticated).Returns(false);
            mock.Setup(service => service.UserName).Returns((string)null);
            mock.Setup(service => service.Email).Returns((string)null);
        }

        
    }
}