namespace CustomerService.Application.Common;

public static class CpfNormalizer
{
    public static string Normalize(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }
}
