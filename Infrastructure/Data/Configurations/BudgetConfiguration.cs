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

            builder.Property(budget => budget.Id).ValueGeneratedOnAdd();

            // ==================== CONFIGURACIÓN DE MONEY ====================
            builder.ComplexProperty
            (
                budget => budget.MonthlyAmount,
                amount =>
                {
                    // ✅ Forzar el uso de campos (para que EF Core use el constructor privado)
                    amount.UsePropertyAccessMode(PropertyAccessMode.Field);

                    amount.Property(m => m.Value)
                        .HasColumnName("MonthlyAmount")
                        .IsRequired()
                        .HasPrecision(18, 2);

                    amount.Property(m => m.Currency)
                        .HasColumnName("Currency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasDefaultValue("EUR");
                }
            );

            // ==================== CONFIGURACIÓN DE PERIOD ====================
            builder.ComplexProperty
            (
                budget => budget.Period,
                period =>
                {
                    period.Property(p => p.Month)
                        .HasColumnName("Month")
                        .IsRequired();

                    period.Property(p => p.Year)
                        .HasColumnName("Year")
                        .IsRequired();
                }
            );

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

            builder.HasOne(budget => budget.User)
                .WithMany(user => user.Budgets)
                .HasForeignKey(budget => budget.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}