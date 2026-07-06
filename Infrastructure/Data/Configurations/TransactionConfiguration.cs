using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            // Tabla
            builder.ToTable("Transactions");

            // Llave primaria
            builder.HasKey(transaction => transaction.Id);

            builder.Property(transaction => transaction.Id)
                .ValueGeneratedOnAdd();

            // ==================== CONFIGURACIÓN DE ENTITYINFO ====================

            builder.OwnsOne
            (
                transaction => transaction.Info, info =>
                {
                    info.Property(i => i.Name)
                        .HasColumnName("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    info.Property(i => i.Description)
                        .HasColumnName("Description")
                        .HasMaxLength(200)
                        .IsRequired(false);
                }
            );

            // ==================== CONFIGURACIÓN DE MONEY ====================

            builder.OwnsOne
            (
                transaction => transaction.Amount, amount =>
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
                }
            );

            // ==================== CONFIGURACIÓN DE DAILYPERIOD ====================

            builder.OwnsOne
            (
                transaction => transaction.Date, date =>
                {
                    date.Property(d => d.Day)
                        .HasColumnName("Day")
                        .IsRequired();

                    date.Property(d => d.Month)
                        .HasColumnName("Month")
                        .IsRequired();

                    date.Property(d => d.Year)
                        .HasColumnName("Year")
                        .IsRequired();

                    // Índices compuestos para consultas rápidas
                    date.HasIndex(d => new { d.Year, d.Month })
                        .HasDatabaseName("IX_Transactions_Year_Month");

                    date.HasIndex(d => new { d.Year, d.Month, d.Day })
                        .HasDatabaseName("IX_Transactions_Year_Month_Day");
                }
            );

            // ==================== PROPIEDADES SIMPLES ====================

            builder.Property(transaction => transaction.Type)
                .HasColumnName("Type")
                .IsRequired();

            builder.Property(transaction => transaction.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(transaction => transaction.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false);

            // ==================== RELACIONES ====================

            builder.HasOne(transaction => transaction.Category)
                .WithMany()
                .HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar una categoría con transacciones

            // ==================== ÍNDICES ====================

            builder.HasIndex(transaction => transaction.CategoryId)
                .HasDatabaseName("IX_Transactions_CategoryId");

            builder.HasIndex(transaction => new { transaction.CategoryId, transaction.Type })
                .HasDatabaseName("IX_Transactions_CategoryId_Type");
        }
    }
}