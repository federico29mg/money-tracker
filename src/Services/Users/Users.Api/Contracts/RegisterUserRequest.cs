namespace Users.Api.Contracts;

public sealed record RegisterUserRequest(string DisplayName, string? DefaultCurrencyCode);