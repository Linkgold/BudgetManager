using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
    {
        public void Configure(EntityTypeBuilder<Expense> builder)
        {
            builder.ToTable("Expenses");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Description)
                .HasMaxLength(200)
                .IsRequired();

            builder.OwnsOne(e => e.Amount, money =>
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

            builder.Property(e => e.DateTime)
                .IsRequired();

            builder.Property(e => e.Category)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(e => e.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            // Relationships
            /*builder.HasOne(e => e.Budget)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);*/

            // Indexes
            builder.HasIndex(e => e.DateTime);
            builder.HasIndex(e => e.BudgetId);
        }
    }
}