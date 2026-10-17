namespace Users.Application.Features.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password, string DisplayName, string? DefaultCurrencyCode)
{
    public override string ToString() =>
        $"{nameof(RegisterUserCommand)} {{ Email = {Email}, DisplayName = {DisplayName}, DefaultCurrencyCode = {DefaultCurrencyCode} }}";
}