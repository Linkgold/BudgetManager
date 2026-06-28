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

            builder.OwnsOne(fixedExpense => fixedExpense.ChargePeriod, period =>
            {
                period.Property(p => p.Year)
                    .HasColumnName("ChargeYear")
                    .IsRequired();

                period.Property(p => p.Month)
                    .HasColumnName("ChargeMonth")
                    .IsRequired();
            });

            // ==================== PROPIEDADES SIMPLES ====================

            builder.Property(fixedExpense => fixedExpense.IsActive)
                .HasColumnName("IsActive")
                .IsRequired()
                .HasDefaultValue(true);

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

            builder.HasIndex(fixedExpense => new { fixedExpense.CategoryId, fixedExpense.IsActive })
                .HasDatabaseName("IX_FixedExpenses_CategoryId_IsActive");

            builder.HasIndex(fixedExpense => new { fixedExpense.ChargePeriod.Year, fixedExpense.ChargePeriod.Month })
                .HasDatabaseName("IX_FixedExpenses_ChargePeriod");

            // Índice compuesto para búsquedas rápidas por período y categoría
            builder.HasIndex(fixedExpense => new {
                fixedExpense.CategoryId,
                fixedExpense.ChargePeriod.Year,
                fixedExpense.ChargePeriod.Month,
                fixedExpense.IsActive
            }).HasDatabaseName("IX_FixedExpenses_Category_Period_Active");
        }
    }
}