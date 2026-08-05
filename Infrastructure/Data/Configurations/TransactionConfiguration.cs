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
            builder.ComplexProperty
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
            builder.ComplexProperty
            (
                transaction => transaction.Amount, amount =>
                {
                    // ✅ Forzar el uso de campos (para que EF Core use el constructor privado)
                    amount.UsePropertyAccessMode(PropertyAccessMode.Field);

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
            builder.ComplexProperty
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
                }
            );

            // ==================== PROPIEDADES SIMPLES ====================
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

            builder.HasOne(transaction => transaction.User)
                .WithMany(user => user.Transactions)
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}