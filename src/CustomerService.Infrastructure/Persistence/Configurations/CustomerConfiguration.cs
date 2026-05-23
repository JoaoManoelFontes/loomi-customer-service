using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(customer => customer.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();

        builder.Property(customer => customer.Address)
            .HasColumnName("address")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(customer => customer.ProfilePictureUrl)
            .HasColumnName("profile_picture_url")
            .HasMaxLength(2048);
    }
}
