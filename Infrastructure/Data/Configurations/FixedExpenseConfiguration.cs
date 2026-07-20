using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class FixedExpenseConfiguration : IEntityTypeConfiguration<FixedExpense>
    {
        public void Configure(EntityTypeBuilder<FixedExpense> builder)
        {
            builder.ToTable("FixedExpenses");

            // Llave primaria
            builder.HasKey(fixedExpense => fixedExpense.Id);

            builder.Property(fixedExpense => fixedExpense.Id)
                .ValueGeneratedOnAdd();

            // ==================== CONFIGURACIÓN DE ENTITYINFO ====================

            builder.OwnsOne(fixedExpense => fixedExpense.Info, info =>
            {
                info.Property(i => i.Name)
                    .HasColumnName("Name")
                    .IsRequired()
                    .HasMaxLength(50);

                info.Property(i => i.Description)
                    .HasColumnName("Description")
                    .HasMaxLength(200)
                    .IsRequired(false);
            });

            // ==================== CONFIGURACIÓN DE MONEY ====================

            builder.OwnsOne(fixedExpense => fixedExpense.Amount, amount =>
            {
                amount.Property(m => m.Value)
                    .HasColumnName("Amount")
                    .IsRequired()
                    .HasPrecision(18, 2);

                amount.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("EUR");
            });

            // ==================== CONFIGURACIÓN DE PERIOD ====================

            builder.OwnsOne
            (
                fixedExpense => fixedExpense.ChargePeriod, 
                period =>
                {
                    period.Property(p => p.Month)
                        .HasColumnName("ChargeMonth")
                        .IsRequired();

                    period.Property(p => p.Year)
                        .HasColumnName("ChargeYear")
                        .IsRequired();

                    // Índice simple sobre el período (para búsquedas por año/mes)
                    period.HasIndex(p => new { p.Month, p.Year })
                    .HasDatabaseName("IX_FixedExpenses_ChargePeriod");
                }
            );

            // ==================== PROPIEDADES SIMPLES ====================

            builder.Property(fixedExpense => fixedExpense.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(fixedExpense => fixedExpense.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false);

            // ==================== RELACIONES ====================

            // Relación con Category (Muchos a Uno)
            builder.HasOne(fixedExpense => fixedExpense.Category)
                .WithMany() // ← Cuando se implementen las navigation properties en Category, se pondrá: .WithMany(category => category.FixedExpenses)
                .HasForeignKey(fixedExpense => fixedExpense.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar una categoría si tiene gastos fijos

            // ==================== ÍNDICES ====================

            builder.HasIndex(fixedExpense => fixedExpense.CategoryId)
                .HasDatabaseName("IX_FixedExpenses_CategoryId");

            // 1. Definir Shadow Properties para el período (para usarlas en índices compuestos)
            builder.Property<int>("Month")
                .HasColumnName("Month")
                .IsRequired();

            builder.Property<int>("Year")
                .HasColumnName("Year")
                .IsRequired();

            // ✅ ÍNDICE COMPUESTO USANDO SHADOW PROPERTIES
            builder.HasIndex("CategoryId", "Month", "Year")
                .HasDatabaseName("IX_FixedExpenses_Category_Period");
        }
    }
}