namespace CustomerService.Application.Customers.Exists;

public sealed class CustomerExistenceCacheOptions
{
    public const string SectionName = "CustomerCache";

    public int ExistsTtlSeconds { get; init; } = 300;

    public int DetailsTtlSeconds { get; init; } = 300;

    public TimeSpan ExistsTtl => TimeSpan.FromSeconds(ExistsTtlSeconds);

    public TimeSpan DetailsTtl => TimeSpan.FromSeconds(DetailsTtlSeconds);
}
