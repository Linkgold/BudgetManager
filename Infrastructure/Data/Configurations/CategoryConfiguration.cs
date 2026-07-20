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

            builder.HasKey(category => category.Id);

            builder.Property(category => category.Id)
                .ValueGeneratedOnAdd();

            builder.OwnsOne
            (
                category => category.Info, 
                info =>
                {
                    info.Property(i => i.Name)
                        .HasColumnName("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    info.Property(i => i.Description)
                        .HasColumnName("Description")
                        .HasMaxLength(500)
                        .IsRequired(false);

                    info.HasIndex(i => i.Name)
                    .IsUnique()
                    .HasDatabaseName("IX_Categories_Name");
                }
            );

            builder.Property(category => category.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(category => category.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false);
        }
    }
}