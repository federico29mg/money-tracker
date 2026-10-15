namespace Users.Application.Features.GetUser;

public sealed record UserResponse(Guid Id, string DisplayName, string DefaultCurrency);