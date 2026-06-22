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

            builder.HasKey(fe => fe.Id);

            builder.Property(fe => fe.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.OwnsOne(fe => fe.Amount, money =>
            {
                money.Property(m => m.Value)
                    .HasColumnName("Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("EUR");
            });

            builder.Property(fe => fe.ChargeMonth)
                .IsRequired();

            builder.Property(fe => fe.Year)
                .IsRequired();

            builder.Property(fe => fe.ChargeDay)
                .IsRequired(false);

            builder.Property(fe => fe.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(fe => fe.IsActive)
                .IsRequired();

            builder.Property(fe => fe.CreatedAt)
                .IsRequired();

            builder.Property(fe => fe.UpdatedAt)
                .IsRequired(false);

            builder.Property(fe => fe.BudgetId)
                .IsRequired(false);

            builder.Property(fe => fe.CategoryId)
                .IsRequired(false);

            // Relationships
            builder.HasOne(fe => fe.Budget)
                .WithMany()
                .HasForeignKey(fe => fe.BudgetId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(fe => fe.Category)
                .WithMany(c => c.FixedExpenses)
                .HasForeignKey(fe => fe.CategoryId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(fe => new { fe.ChargeMonth, fe.Year });
            builder.HasIndex(fe => fe.BudgetId);
            builder.HasIndex(fe => fe.CategoryId);
            builder.HasIndex(fe => fe.IsActive);
        }
    }
}