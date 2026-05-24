using CustomerService.Application.Abstractions;
using CustomerService.Application.Common;
using CustomerService.Domain.Entities;
using CustomerService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerService.Infrastructure.Persistence;

public static class DevelopmentAdminSeeder
{
    public const string DemoAdminCpf = "00000000000";
    public const string DemoAdminPassword = "Admin@123";

    public static async Task SeedDevelopmentAdminAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        var cpf = CpfNormalizer.Normalize(DemoAdminCpf);

        if (await dbContext.Users.AnyAsync(user => user.Cpf == cpf, cancellationToken))
        {
            return;
        }

        dbContext.Users.Add(new User(cpf, passwordHasher.Hash(DemoAdminPassword), UserRole.Admin));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
