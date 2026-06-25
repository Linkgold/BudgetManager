using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// Contexto principal de Entity Framework Core
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<FixedExpense> FixedExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
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