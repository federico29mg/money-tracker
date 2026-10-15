namespace Users.Application.Features.RegisterUser;

public sealed record RegisterUserCommand(string DisplayName, string? DefaultCurrencyCode);