namespace Users.Application.Features.ChangeDefaultCurrency;

public sealed record ChangeDefaultCurrencyCommand(Guid UserId, string CurrencyCode);