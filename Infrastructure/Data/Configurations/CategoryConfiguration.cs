using Contracts.Enums;
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

            builder.Property(category => category.Id).ValueGeneratedOnAdd();

            // ==================== CONFIGURACIÓN DE ENTITYINFO ====================
            builder.ComplexProperty
            (
                category => category.Info,
                info =>
                {
                    info.Property(i => i.Name)
                        .HasColumnName("Name")
                        .HasMaxLength(50)
                        .IsRequired();

                    info.Property(i => i.Description)
                        .HasColumnName("Description")
                        .HasMaxLength(200);
                }
            );

            /* TODO: Esperar a la nueva versión de EF para crear un índice único para el par (UserId, Info.Name)
                     Ahora mismo, EF Core no permite crear un índice único para una propiedad compleja (Info.Name) junto con otra propiedad (UserId).
            /*builder.HasIndex("UserId", "Info_Name")
                .IsUnique()
                .HasDatabaseName("IX_Categories_UserId_Name");*/

            // ==================== PROPIEDADES SIMPLES ====================
            builder.Property(category => category.Nature)
                .HasColumnName("Nature")
                .IsRequired()
                .HasDefaultValue(CategoryNatureEnum.Expense); // ✅ Valor por defecto en BD

            builder.Property(category => category.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(category => category.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false);

            // ==================== RELACIONES ====================
            builder.HasOne(category => category.User)
                .WithMany(user => user.Categories)
                .HasForeignKey(category => category.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}