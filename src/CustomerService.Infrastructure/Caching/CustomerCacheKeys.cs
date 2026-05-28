namespace CustomerService.Infrastructure.Caching;

internal static class CustomerCacheKeys
{
    public static string Exists(Guid customerId)
    {
        return $"customers:{customerId}:exists";
    }

    public static string Details(Guid customerId)
    {
        return $"customers:{customerId}:details";
    }
}
