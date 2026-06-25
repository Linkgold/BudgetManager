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

            builder.HasKey(b => b.Id);

            builder.OwnsOne(b => b.MonthlyAmount, money =>
            {
                money.Property(m => m.Value)
                    .HasColumnName("MonthlyAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired()
                    .HasDefaultValue("EUR");
            });

            builder.OwnsOne(b => b.Period, period =>
            {
                period.Property(p => p.Month)
                    .HasColumnName("Month")
                    .IsRequired();

                period.Property(p => p.Year)
                    .HasColumnName("Year")
                    .IsRequired();

                // Indexes for Period properties
                period.HasIndex(p => p.Month);
                period.HasIndex(p => p.Year);
            });

            builder.Property(b => b.CreatedAt)
                .IsRequired();

            builder.Property(b => b.UpdatedAt)
                .IsRequired(false);

            // Relationships
            /*builder.HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);*/

            // Indexes
            builder.HasIndex(b => b.CategoryId);
        }
    }
}