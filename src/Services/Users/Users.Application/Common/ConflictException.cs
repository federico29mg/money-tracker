namespace Users.Application.Common;

public class ConflictException(string message) : Exception(message);