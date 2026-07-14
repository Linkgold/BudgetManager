using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(user => user.Id);

            builder.Property(user => user.Id)
                .ValueGeneratedOnAdd();

            // ==================== CONFIGURACIÓN DE USERINFO ====================

            builder.OwnsOne(user => user.Info, info =>
            {
                info.Property(i => i.UserName)
                    .HasColumnName("UserName")
                    .IsRequired()
                    .HasMaxLength(50);

                info.Property(i => i.Email)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // ==================== PROPIEDADES SIMPLES ====================

            builder.Property(user => user.PasswordHash)
                .HasColumnName("PasswordHash")
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(user => user.IsActive)
                .HasColumnName("IsActive")
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(user => user.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(user => user.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .IsRequired(false);

            // ==================== ÍNDICES ====================

            builder.OwnsOne(user => user.Info, info =>
            {
                info.Property(i => i.Email)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(100);

                info.Property(i => i.UserName)
                    .HasColumnName("UserName")
                    .IsRequired()
                    .HasMaxLength(50);

                // Índice simple para Email (dentro del OwnsOne)
                info.HasIndex(i => i.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                // Índice simple para UserName (dentro del OwnsOne)
                info.HasIndex(i => i.UserName)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_UserName");
            });

            // Índice para IsActive (búsquedas de usuarios activos)
            builder.HasIndex(user => user.IsActive)
                .HasDatabaseName("IX_Users_IsActive");
        }
    }
}
