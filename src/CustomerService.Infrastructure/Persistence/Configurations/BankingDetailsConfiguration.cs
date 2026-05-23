using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerService.Infrastructure.Persistence.Configurations;

public sealed class BankingDetailsConfiguration : IEntityTypeConfiguration<BankingDetails>
{
    public void Configure(EntityTypeBuilder<BankingDetails> builder)
    {
        builder.ToTable("banking_details");

        builder.HasKey(bankingDetails => bankingDetails.Id);

        builder.Property(bankingDetails => bankingDetails.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(bankingDetails => bankingDetails.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(bankingDetails => bankingDetails.Agency)
            .HasColumnName("agency")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(bankingDetails => bankingDetails.CheckingAccountNumber)
            .HasColumnName("checking_account_number")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(bankingDetails => bankingDetails.Balance)
            .HasColumnName("balance")
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
