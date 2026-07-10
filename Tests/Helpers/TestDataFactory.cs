using Application.DTOs.Request;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using Moq;
using System.Runtime.CompilerServices;

namespace Tests.Helpers
{
    /// <summary>
    /// Fábrica de datos para pruebas unitarias de todas las capas
    /// </summary>
    public static class TestDataFactory
    {
        // ==================== USUARIOS ====================

        /// <summary>
        /// Crea un usuario de prueba con ID
        /// </summary>
        public static User CreateUser(int id = 1, string userName = "Juan Pérez", string email = "juan@email.com")
        {
            UserInfo userInfo = new UserInfo(userName, email);
            User user = new User(userInfo, "hashed_password");
            typeof(User).GetProperty("Id")?.SetValue(user, id);

            return user;
        }

        /// <summary>
        /// Crea un usuario con contraseña hasheada (para pruebas de login)
        /// </summary>
        public static User CreateUserWithPassword(string password = "Password123!", int id = 1, string email = "juan@email.com")
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            UserInfo userInfo = new UserInfo("Juan Pérez", email);
            User user = new User(userInfo, passwordHash);
            typeof(User).GetProperty("Id")?.SetValue(user, id);

            return user;
        }

        // ==================== CATEGORÍAS ====================

        /// <summary>
        /// Crea una categoría de prueba con usuario
        /// </summary>
        public static Category CreateCategory(int id = 1, User user = null, string name = "Alimentación", string description = "Gastos de comida")
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
                categories.Add(CreateCategory(i, user, $"Categoría {i}", $"Descripción {i}"));
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
            string name = "Netflix",
            string description = "Suscripción mensual",
            decimal amount = 15.99m,
            int month = 1,
            int year = 2024
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

        // ==================== PRESUPUESTOS ====================

        /// <summary>
        /// Crea un presupuesto de prueba
        /// </summary>
        public static Budget CreateBudget
        (
            int id = 1,
            User user = null,
            Category category = null,
            decimal monthlyAmount = 500.00m,
            int month = 1,
            int year = 2024
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

        // ==================== TRANSACCIONES ====================

        /// <summary>
        /// Crea una transacción de prueba
        /// </summary>
        public static Transaction CreateTransaction
        (
            int id = 1,
            User user = null,
            Category category = null,
            string name = "Compra supermercado",
            string description = "Carrefour 15/06/2024",
            decimal amount = 45.75m,
            TransactionTypeEnum type = TransactionTypeEnum.Expense,
            int day = 15,
            int month = 6,
            int year = 2024
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

        // ==================== VALUE OBJECTS ====================

        /// <summary>
        /// Crea un EntityInfo de prueba
        /// </summary>
        public static EntityInfo CreateEntityInfo(string name = "Test", string description = "Test Description") => new EntityInfo(name, description);

        /// <summary>
        /// Crea un UserInfo de prueba
        /// </summary>
        public static UserInfo CreateUserInfo(string userName = "Juan Pérez", string email = "juan@email.com") => new UserInfo(userName, email);

        /// <summary>
        /// Crea un DailyPeriod de prueba
        /// </summary>
        public static DailyPeriod CreateDailyPeriod(int day = 15, int month = 6, int year = 2024) => new DailyPeriod(day, month, year);

        /// <summary>
        /// Crea un MonthlyPeriod de prueba
        /// </summary>
        public static MonthlyPeriod CreateMonthlyPeriod(int month = 1, int year = 2024) => new MonthlyPeriod(month, year);

        /// <summary>
        /// Crea un Money de prueba
        /// </summary>
        public static Money CreateMoney(decimal value = 100.00m, string currency = "EUR") => new Money(value, currency);

        // ==================== DTOs DE PRUEBA (para Application) ====================

        /// <summary>
        /// Crea un CreateCategoryRequestDTO de prueba
        /// </summary>
        public static CreateCategoryRequestDTO CreateCategoryRequest(string name = "Alimentación", string description = "Gastos de comida") => new CreateCategoryRequestDTO { Name = name, Description = description };

        /// <summary>
        /// Crea un CreateFixedExpenseRequestDTO de prueba
        /// </summary>
        public static CreateFixedExpenseRequestDTO CreateFixedExpenseRequest
        (
            int categoryId = 1,
            string name = "Netflix",
            string description = "Suscripción mensual",
            decimal amount = 15.99m,
            int month = 1,
            int year = 2024
        )
        {
            return new CreateFixedExpenseRequestDTO
            {
                CategoryId = categoryId,
                Name = name,
                Description = description,
                Amount = amount,
                Month = month,
                Year = year
            };
        }

        /// <summary>
        /// Crea un CreateBudgetRequestDTO de prueba
        /// </summary>
        public static CreateBudgetRequestDTO CreateBudgetRequest
        (
            int categoryId = 1,
            decimal amount = 500.00m,
            int month = 1,
            int year = 2024
        )
        {
            return new CreateBudgetRequestDTO
            {
                CategoryId = categoryId,
                Amount = amount,
                Month = month,
                Year = year
            };
        }

        /// <summary>
        /// Crea un CreateTransactionRequestDTO de prueba
        /// </summary>
        public static CreateTransactionRequestDTO CreateTransactionRequest
        (
            int categoryId = 1,
            string name = "Compra supermercado",
            string description = "Carrefour 15/06/2024",
            decimal amount = 45.75m,
            TransactionTypeEnum type = TransactionTypeEnum.Expense,
            DateTime? date = null
        )
        {
            date ??= new DateTime(2024, 6, 15);
            return new CreateTransactionRequestDTO
            {
                CategoryId = categoryId,
                Name = name,
                Description = description,
                Amount = amount,
                Type = type,
                Date = date.Value
            };
        }

        /// <summary>
        /// Crea un CreateUserRequestDTO de prueba
        /// </summary>
        public static CreateUserRequestDTO CreateUserRequest
        (
            string userName = "Juan Pérez",
            string email = "juan@email.com",
            string password = "Password123!",
            string confirmPassword = "Password123!"
        )
        {
            return new CreateUserRequestDTO
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };
        }

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