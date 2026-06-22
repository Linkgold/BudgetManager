using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{

    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(c => c.IsActive)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(c => c.Name).IsUnique();
            builder.HasIndex(c => c.IsActive);

            // Property for HasData with Id assignment
            builder.Property(c => c.Id).ValueGeneratedNever();

            // Seed data
            builder.HasData(
                new { Id = 1, Name = "Food", Description = "Groceries and meals", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 2, Name = "Leisure", Description = "Entertainment and outings", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 3, Name = "Transport", Description = "Fuel, public transport and maintenance", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 4, Name = "Housing", Description = "Rent, mortgage and utilities", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 5, Name = "Health", Description = "Insurance, pharmacy and medical visits", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 6, Name = "Education", Description = "Courses, books and training", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 7, Name = "Clothing", Description = "Clothes and footwear", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 8, Name = "Insurance", Description = "Various insurances", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 9, Name = "Services", Description = "Subscriptions and various services", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 10, Name = "Savings", Description = "Savings and investments", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null },
                new { Id = 11, Name = "Other", Description = "Miscellaneous expenses", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = (DateTime?)null }
            );
        }
    }
}