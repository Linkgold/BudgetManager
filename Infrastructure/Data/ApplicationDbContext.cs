using Domain.Entities;
using Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// Contexto principal de Entity Framework Core
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<FixedExpense> FixedExpenses { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new FixedExpenseConfiguration());
            modelBuilder.ApplyConfiguration(new BudgetConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }

        /// <summary>
        /// Método para asegurar que la base de datos está creada
        /// </summary>
        public void EnsureDatabaseCreated()
        {
            Database.EnsureCreated();

            CreateIndexes(this);
        }

        /// <summary>
        /// Método para aplicar migraciones pendientes
        /// </summary>
        public void MigrateDatabase() => Database.Migrate();


        private static void CreateIndexes(ApplicationDbContext dbContext)
        {
            CreateBudgetIndexes(dbContext);
            CreateCategoryIndexes(dbContext);
            CreateFixedExpenseIndexes(dbContext);
            CreateTransactionIndexes(dbContext);
            CreateUserIndexes(dbContext);
        }

        private static void CreateBudgetIndexes(ApplicationDbContext dbContext)
        {
            dbContext.Database.ExecuteSqlRaw
            (
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_Budgets_User_Category_Period ON Budgets(UserId, CategoryId, Month, Year);"
            );

            dbContext.Database.ExecuteSqlRaw
            (
                "CREATE INDEX IF NOT EXISTS IX_Budgets_User_Period ON Budgets(UserId, Month, Year);"
            );
        }

        private static void CreateCategoryIndexes(ApplicationDbContext dbContext)
        {
            dbContext.Database.ExecuteSqlRaw
            (
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_Categories_User_Name ON Categories(UserId, Name);"
            );
        }

        private static void CreateFixedExpenseIndexes(ApplicationDbContext dbContext)
        {
            dbContext.Database.ExecuteSqlRaw
            (
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_FixedExpenses_User_Category_Name_Period ON FixedExpenses(UserId, CategoryId, Name, Month, Year);"
            );

            dbContext.Database.ExecuteSqlRaw
            (
                "CREATE INDEX IF NOT EXISTS IX_FixedExpenses_User_Period ON FixedExpenses(UserId, Month, Year);"
            );
        }

        private static void CreateTransactionIndexes(ApplicationDbContext dbContext)
        {
            dbContext.Database.ExecuteSqlRaw
            (
                "CREATE INDEX IF NOT EXISTS IX_Transactions_User_Period ON Transactions(UserId, Month, Year);"
            );
        }

        private static void CreateUserIndexes(ApplicationDbContext dbContext)
        {
            dbContext.Database.ExecuteSqlRaw
            (
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);"
            );
        }
    }
}