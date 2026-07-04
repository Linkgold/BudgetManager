using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.ToTable("Budgets");

            builder.HasKey(budget => budget.Id);
            
            builder.Property(budget => budget.Id)
                .ValueGeneratedOnAdd();

            // ==================== CONFIGURACIÓN DE MONEY ====================
            builder.OwnsOne(budget => budget.MonthlyAmount, amount =>
            {
                amount.Property(m => m.Value)
                    .HasColumnName("MonthlyAmount")
                    .IsRequired()
                    .HasPrecision(18, 2);

                amount.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("EUR");
            });

            // ==================== CONFIGURACIÓN DE PERIOD ====================
            // 1. Definir Shadow Properties para el período (para usarlas en índices compuestos)
            builder.Property<int>("Month")
                .HasColumnName("Month")
                .IsRequired();

            builder.Property<int>("Year")
                .HasColumnName("Year")
                .IsRequired();

            // 2. Configurar el Value Object para que use las Shadow Properties
            builder.OwnsOne(budget => budget.Period, period =>
            {
                period.Property(p => p.Month)
                    .HasColumnName("Month")
                    .IsRequired();

                period.Property(p => p.Year)
                    .HasColumnName("Year")
                    .IsRequired();

                // Índice simple para el período
                period.HasIndex(p => new { p.Month, p.Year  })
                    .HasDatabaseName("IX_Budgets_Period");
            });

            // ==================== PROPIEDADES SIMPLES ====================
            builder.Property(budget => budget.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(budget => budget.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false);

            // ==================== RELACIONES ====================
            builder.HasOne(budget => budget.Category)
                .WithMany()
                .HasForeignKey(budget => budget.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==================== ÍNDICES ====================
            builder.HasIndex(budget => budget.CategoryId)
                .HasDatabaseName("IX_Budgets_CategoryId");

            /*// Índice único: Una categoría solo puede tener un presupuesto por período
            builder.HasIndex(budget => new { budget.CategoryId, budget.Period.Year, budget.Period.Month })
                .IsUnique()
                .HasDatabaseName("IX_Budgets_Category_Period");*/

            // ✅ ÍNDICE ÚNICO USANDO NOMBRES DE COLUMNA (NO con propiedades anidadas)
            builder.HasIndex("CategoryId", "Year", "Month")
                .IsUnique()
                .HasDatabaseName("IX_Budgets_Category_Period");
        }
    }
}