using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Persistence;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<BankingDetails> BankingDetails => Set<BankingDetails>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
