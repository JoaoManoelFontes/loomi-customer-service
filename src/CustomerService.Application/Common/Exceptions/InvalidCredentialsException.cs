namespace CustomerService.Application.Common.Exceptions;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid CPF or password.")
    {
    }
}
