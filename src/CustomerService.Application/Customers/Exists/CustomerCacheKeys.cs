namespace CustomerService.Application.Customers.Exists;

public static class CustomerCacheKeys
{
    public static string Exists(Guid customerId)
    {
        return $"customer:{customerId:D}";
    }
}
