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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new FixedExpenseConfiguration());
            modelBuilder.ApplyConfiguration(new BudgetConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        }

        /// <summary>
        /// Método para asegurar que la base de datos está creada
        /// </summary>
        public void EnsureDatabaseCreated()
        {
            this.Database.EnsureCreated();
        }

        /// <summary>
        /// Método para aplicar migraciones pendientes
        /// </summary>
        public void MigrateDatabase()
        {
            this.Database.Migrate();
        }
    }
}