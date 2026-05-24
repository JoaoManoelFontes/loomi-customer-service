using CustomerService.Domain.Entities;
using CustomerService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(user => user.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(user => user.Cpf)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasColumnName("role")
            .HasMaxLength(30)
            .HasConversion(
                role => role.ToString(),
                value => Enum.Parse<UserRole>(value))
            .IsRequired();

        builder.Property(user => user.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(user => user.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
