namespace Transactions.Application.Common;

public class NotFoundException(string message) : Exception(message);