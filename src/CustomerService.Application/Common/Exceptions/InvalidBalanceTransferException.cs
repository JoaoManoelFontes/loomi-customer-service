namespace CustomerService.Application.Common.Exceptions;

public sealed class InvalidBalanceTransferException(string message) : Exception(message);
