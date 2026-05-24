namespace CustomerService.Application.Common.Exceptions;

public sealed class DuplicateCpfException : Exception
{
    public DuplicateCpfException(string cpf)
        : base($"CPF '{cpf}' is already in use.")
    {
    }
}
