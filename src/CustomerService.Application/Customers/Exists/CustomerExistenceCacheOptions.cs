namespace CustomerService.Application.Customers.Exists;

public sealed class CustomerExistenceCacheOptions
{
    public const string SectionName = "CustomerCache";

    public int ExistsTtlSeconds { get; init; } = 300;

    public TimeSpan ExistsTtl => TimeSpan.FromSeconds(ExistsTtlSeconds);
}
